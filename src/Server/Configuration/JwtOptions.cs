// Configuration settings describing how JWT tokens are issued and validated.
namespace NetAppForVika.Server.Configuration;

/// <summary>
/// Options describing JWT issuer, audience, and signing credentials.
/// </summary>
public sealed class JwtOptions
{
    /// <summary>
    /// Issuer string embedded in generated tokens.
    /// </summary>
    public string Issuer { get; set; } = "NetAppForVika";

    /// <summary>
    /// Audience allowed to consume the tokens.
    /// </summary>
    public string Audience { get; set; } = "NetAppForVika.Client";

    /// <summary>
    /// Symmetric signing key used to sign and validate JWTs.
    /// </summary>
    public string SigningKey { get; set; } = "change-me-in-production-very-secret";

    /// <summary>
    /// Token lifetime in minutes.
    /// </summary>
    public int ExpirationMinutes { get; set; } = 120;
}
