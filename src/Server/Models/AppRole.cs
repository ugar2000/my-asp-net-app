// Identity role used to grant elevated privileges such as news authoring.
using Microsoft.AspNetCore.Identity;

namespace NetAppForVika.Server.Models;

/// <summary>
/// Customizable role record.
/// </summary>
public sealed class AppRole : IdentityRole
{
    /// <summary>
    /// Human-readable description of the role's responsibilities.
    /// </summary>
    public string? Description { get; set; }
}
