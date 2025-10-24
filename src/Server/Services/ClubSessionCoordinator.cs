// Coordinates collaborative session state across Redis cache and the relational database.
using System.Collections.Immutable;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using NetAppForVika.Server.Abstractions;
using NetAppForVika.Server.Data;
using NetAppForVika.Server.Models;
using NetAppForVika.Shared.Dtos;

namespace NetAppForVika.Server.Services;

/// <summary>
/// Manages collaborative lesson state to ensure quick updates and durable recovery.
/// </summary>
public sealed class ClubSessionCoordinator : IClubSessionCoordinator
{
    private const string CachePrefix = "club-session:";

    private readonly AppDbContext _dbContext;
    private readonly IDistributedCache _cache;
    private readonly ILogger<ClubSessionCoordinator> _logger;

    /// <summary>
    /// Initializes the coordinator with EF Core, Redis cache, and logging.
    /// </summary>
    public ClubSessionCoordinator(
        AppDbContext dbContext,
        IDistributedCache cache,
        ILogger<ClubSessionCoordinator> logger)
    {
        _dbContext = dbContext;
        _cache = cache;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<ClubSessionStateDto> GetSessionAsync(string sessionId, CancellationToken cancellationToken)
    {
        var cacheKey = CachePrefix + sessionId;
        var cached = await _cache.GetStringAsync(cacheKey, cancellationToken);
        if (!string.IsNullOrWhiteSpace(cached))
        {
            return Deserialize(cached);
        }

        var snapshot = await _dbContext.ClubSessions
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.SessionId == sessionId, cancellationToken);

        var state = snapshot is not null
            ? new ClubSessionStateDto(
                snapshot.SessionId,
                snapshot.CodeDocument,
                snapshot.ConsoleOutput,
                ParseParticipants(snapshot.ParticipantsJson))
            : BuildDefaultState(sessionId);

        await CacheAsync(cacheKey, state, cancellationToken);
        return state;
    }

    /// <inheritdoc />
    public async Task<ClubSessionStateDto> ApplyUpdateAsync(
        ClubSessionUpdateDto update,
        CancellationToken cancellationToken)
    {
        var state = await GetSessionAsync(update.SessionId, cancellationToken);
        var updatedCode = ApplyDelta(state.CodeDocument, update.EditorDelta);
        var updatedOutput = string.IsNullOrWhiteSpace(update.OutputAppend)
            ? state.ConsoleOutput
            : string.Concat(state.ConsoleOutput, "\n", update.OutputAppend);

        var updatedParticipants = UpdateParticipants(state.Participants, update.Author);
        var nextState = state with
        {
            CodeDocument = updatedCode,
            ConsoleOutput = updatedOutput,
            Participants = updatedParticipants
        };

        await PersistAsync(nextState, cancellationToken);
        return nextState;
    }

    /// <summary>
    /// Persists the latest session snapshot to both Redis and PostgreSQL.
    /// </summary>
    private async Task PersistAsync(ClubSessionStateDto state, CancellationToken cancellationToken)
    {
        var cacheKey = CachePrefix + state.SessionId;
        await CacheAsync(cacheKey, state, cancellationToken);

        var existing = await _dbContext.ClubSessions
            .FirstOrDefaultAsync(x => x.SessionId == state.SessionId, cancellationToken);

        if (existing is null)
        {
            existing = new ClubSessionSnapshot
            {
                SessionId = state.SessionId
            };
            _dbContext.ClubSessions.Add(existing);
        }

        existing.CodeDocument = state.CodeDocument;
        existing.ConsoleOutput = state.ConsoleOutput;
        existing.ParticipantsJson = JsonSerializer.Serialize(state.Participants, SerializerOptions);
        existing.UpdatedAtUtc = DateTimeOffset.UtcNow;

        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    /// <summary>
    /// Writes the current session state to Redis with conservative expiration policies.
    /// </summary>
    private async Task CacheAsync(string cacheKey, ClubSessionStateDto state, CancellationToken cancellationToken)
    {
        var payload = JsonSerializer.Serialize(state, SerializerOptions);
        await _cache.SetStringAsync(cacheKey, payload, new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(60),
            SlidingExpiration = TimeSpan.FromMinutes(10)
        }, cancellationToken);
    }

    /// <summary>
    /// Deserialises cached session payloads using the shared JSON profile.
    /// </summary>
    private static ClubSessionStateDto Deserialize(string payload)
    {
        var state = JsonSerializer.Deserialize<ClubSessionStateDto>(payload, SerializerOptions);
        return state ?? throw new InvalidOperationException("Failed to deserialize cached club session state.");
    }

    /// <summary>
    /// Converts the stored participant JSON into an immutable roster.
    /// </summary>
    private static ImmutableArray<ClubSessionParticipantDto> ParseParticipants(string json)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            return ImmutableArray<ClubSessionParticipantDto>.Empty;
        }

        try
        {
            var participants = JsonSerializer.Deserialize<ClubSessionParticipantDto[]>(json, SerializerOptions);
            return participants?.ToImmutableArray() ?? ImmutableArray<ClubSessionParticipantDto>.Empty;
        }
        catch (JsonException)
        {
            return ImmutableArray<ClubSessionParticipantDto>.Empty;
        }
    }

    /// <summary>
    /// Applies the latest editor delta, accepting either JSON patches or full texts.
    /// </summary>
    private static string ApplyDelta(string currentDocument, string editorDelta)
    {
        if (string.IsNullOrWhiteSpace(editorDelta))
        {
            return currentDocument;
        }

        try
        {
            using var json = JsonDocument.Parse(editorDelta);
            if (json.RootElement.TryGetProperty("fullText", out var value) &&
                value.ValueKind == JsonValueKind.String)
            {
                return value.GetString() ?? currentDocument;
            }
        }
        catch (JsonException)
        {
            // Fallback to treating the delta as a literal document body.
        }

        return editorDelta;
    }

    /// <summary>
    /// Augments the participant roster when a new author appears.
    /// </summary>
    private static ImmutableArray<ClubSessionParticipantDto> UpdateParticipants(
        ImmutableArray<ClubSessionParticipantDto> participants,
        string author)
    {
        if (string.IsNullOrWhiteSpace(author))
        {
            return participants;
        }

        var existingIndex = -1;
        for (var i = 0; i < participants.Length; i++)
        {
            if (string.Equals(participants[i].DisplayName, author, StringComparison.OrdinalIgnoreCase))
            {
                existingIndex = i;
                break;
            }
        }

        if (existingIndex >= 0)
        {
            return participants;
        }

        var builder = participants.ToBuilder();
        builder.Add(new ClubSessionParticipantDto(author, participants.IsEmpty));
        return builder.ToImmutable();
    }

    /// <summary>
    /// Constructs the default collaborative lesson template for brand-new sessions.
    /// </summary>
    private static ClubSessionStateDto BuildDefaultState(string sessionId)
    {
        const string starterCode = """
                                   // Welcome to Club Mode. The leader can run this script using the Run button.
                                   using System;

                                   Console.WriteLine("Where Code Becomes Science.");
                                   """;

        return new ClubSessionStateDto(
            sessionId,
            starterCode,
            string.Empty,
            ImmutableArray<ClubSessionParticipantDto>.Empty);
    }

    private static readonly JsonSerializerOptions SerializerOptions = new(JsonSerializerDefaults.Web);
}
