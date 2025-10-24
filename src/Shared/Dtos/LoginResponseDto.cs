// Response returned after a successful login.
namespace NetAppForVika.Shared.Dtos;

/// <summary>
/// JWT token and profile details returned to the Blazor client.
/// </summary>
public sealed record LoginResponseDto(
    string Token,
    string DisplayName,
    string Email,
    string[] Roles,
    DateTimeOffset ExpiresAtUtc);
