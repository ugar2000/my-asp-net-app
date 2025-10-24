// SignalR hub wiring collaborative editing events to the coordinator service and broadcasting updates.
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using NetAppForVika.Server.Abstractions;
using NetAppForVika.Shared.Dtos;

namespace NetAppForVika.Server.Hubs;

/// <summary>
/// Handles Club Mode realtime collaboration including roster updates and code synchronisation.
/// </summary>
public sealed class ClubSessionHub : Hub
{
    private readonly IClubSessionCoordinator _coordinator;
    private readonly ILogger<ClubSessionHub> _logger;

    /// <summary>
    /// Injects the coordinator that persists session state into Redis and PostgreSQL.
    /// </summary>
    public ClubSessionHub(
        IClubSessionCoordinator coordinator,
        ILogger<ClubSessionHub> logger)
    {
        _coordinator = coordinator;
        _logger = logger;
    }

    /// <summary>
    /// Adds the caller to the session group and replays the latest shared state.
    /// </summary>
    public async Task JoinSessionAsync(string sessionId, string displayName)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, sessionId);
        var snapshot = await _coordinator.GetSessionAsync(sessionId, Context.ConnectionAborted);
        await Clients.Caller.SendAsync("SessionHydrated", snapshot);

        _logger.LogInformation("Client {ConnectionId} joined session {SessionId} as {DisplayName}.",
            Context.ConnectionId,
            sessionId,
            displayName);
    }

    /// <summary>
    /// Applies an incoming delta and broadcasts the updated state to all participants.
    /// </summary>
    public async Task PushUpdateAsync(ClubSessionUpdateDto update)
    {
        var state = await _coordinator.ApplyUpdateAsync(update, Context.ConnectionAborted);
        await Clients.Group(update.SessionId).SendAsync("SessionUpdated", state);
    }
}
