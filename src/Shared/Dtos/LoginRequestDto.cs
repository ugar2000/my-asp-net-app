// Payload capturing login credentials for JWT issuance.
namespace NetAppForVika.Shared.Dtos;

/// <summary>
/// Login form submission.
/// </summary>
public sealed class LoginRequestDto
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}
