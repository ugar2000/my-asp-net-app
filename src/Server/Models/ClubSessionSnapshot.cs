// EF Core entity providing persistence for collaborative session history when Redis is cleared.
using System.ComponentModel.DataAnnotations;

namespace NetAppForVika.Server.Models;

/// <summary>
/// Captures the latest known state of a collaborative coding session for resilience and analytics.
/// </summary>
public sealed class ClubSessionSnapshot
{
    /// <summary>
    /// Auto-generated identifier for relational integrity.
    /// </summary>
    [Key]
    public Guid Id { get; set; }

    /// <summary>
    /// Human-readable session identifier shared with clients.
    /// </summary>
    [MaxLength(64)]
    public string SessionId { get; set; } = string.Empty;

    /// <summary>
    /// Most recent code document body so learners can resume mid-lesson.
    /// </summary>
    public string CodeDocument { get; set; } = string.Empty;

    /// <summary>
    /// Latest aggregated console output captured from Roslyn scripting runs.
    /// </summary>
    public string ConsoleOutput { get; set; } = string.Empty;

    /// <summary>
    /// ISO-8601 string listing the connected participants mainly for timeline analytics.
    /// </summary>
    public string ParticipantsJson { get; set; } = "[]";

    /// <summary>
    /// Timestamp indicating when the snapshot was last updated.
    /// </summary>
    public DateTimeOffset UpdatedAtUtc { get; set; }
}
