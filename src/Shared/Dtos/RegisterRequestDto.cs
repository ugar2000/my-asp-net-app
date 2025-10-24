// Request payload for creating a new user account.
namespace NetAppForVika.Shared.Dtos;

/// <summary>
/// Registration details for the self-service signup endpoint.
/// </summary>
public sealed class RegisterRequestDto
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string? Bio { get; set; }
}
