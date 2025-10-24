// Shared representation of participants so both server and clients can render rosters predictably.
namespace NetAppForVika.Shared.Dtos;

/// <summary>
/// Snapshot of a live participant connected to the collaborative coding session.
/// </summary>
/// <param name="DisplayName">Friendly name chosen by the learner.</param>
/// <param name="IsLeader">True when the participant is controlling the session flow.</param>
public sealed record ClubSessionParticipantDto(
    string DisplayName,
    bool IsLeader);
