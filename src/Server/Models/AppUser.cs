// Identity user extended with profile metadata for Codex Club.
using Microsoft.AspNetCore.Identity;

namespace NetAppForVika.Server.Models;

/// <summary>
/// Application user leveraging ASP.NET Core Identity.
/// </summary>
public sealed class AppUser : IdentityUser
{
    /// <summary>
    /// Friendly display name rendered in the UI.
    /// </summary>
    public string DisplayName { get; set; } = string.Empty;

    /// <summary>
    /// Short bio rendered on profile cards.
    /// </summary>
    public string? Bio { get; set; }
}
