// Primary state object broadcast over SignalR during collaborative lessons.
using System.Collections.Immutable;

namespace NetAppForVika.Shared.Dtos;

/// <summary>
/// Aggregates the shared code and output buffers so every attendee sees the same lesson frame.
/// </summary>
/// <param name="SessionId">Stable identifier that binds SignalR groups and cache entries.</param>
/// <param name="CodeDocument">Latest collaborative code text stored in the server cache.</param>
/// <param name="ConsoleOutput">Running output from the Roslyn scripting engine or sample runner.</param>
/// <param name="Participants">Ordered roster of participants with leader metadata.</param>
public sealed record ClubSessionStateDto(
    string SessionId,
    string CodeDocument,
    string ConsoleOutput,
    ImmutableArray<ClubSessionParticipantDto> Participants);
