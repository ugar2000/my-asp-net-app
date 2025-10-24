// Lightweight DTO describing the authenticated user profile.
namespace NetAppForVika.Shared.Dtos;

/// <summary>
/// Snapshot of the current user's public profile.
/// </summary>
public sealed record UserProfileDto(
    string Id,
    string DisplayName,
    string Email,
    string? Bio,
    string[] Roles);
