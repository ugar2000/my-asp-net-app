// Entity representing WordPress-style news posts authored by admins.
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NetAppForVika.Server.Models;

/// <summary>
/// News article surfaced on the public blog page.
/// </summary>
public sealed class NewsArticle
{
    /// <summary>
    /// Primary key identifier.
    /// </summary>
    [Key]
    public Guid Id { get; set; }

    /// <summary>
    /// Title shown in the news feed.
    /// </summary>
    [Required]
    [MaxLength(180)]
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// SEO-friendly slug for routing.
    /// </summary>
    [Required]
    [MaxLength(200)]
    public string Slug { get; set; } = string.Empty;

    /// <summary>
    /// Hero summary displayed in cards.
    /// </summary>
    [MaxLength(320)]
    public string Excerpt { get; set; } = string.Empty;

    /// <summary>
    /// Markdown body for the article.
    /// </summary>
    [Column(TypeName = "text")]
    public string Content { get; set; } = string.Empty;

    /// <summary>
    /// Optional cover image URL hosted by the application.
    /// </summary>
    [MaxLength(512)]
    public string? CoverImageUrl { get; set; }

    /// <summary>
    /// Author display name (denormalised for quick rendering).
    /// </summary>
    [MaxLength(128)]
    public string AuthorName { get; set; } = string.Empty;

    /// <summary>
    /// Author user identifier for backoffice lookups.
    /// </summary>
    [MaxLength(450)]
    public string AuthorId { get; set; } = string.Empty;

    /// <summary>
    /// Published timestamp.
    /// </summary>
    public DateTimeOffset PublishedAtUtc { get; set; }

    /// <summary>
    /// Last updated timestamp.
    /// </summary>
    public DateTimeOffset UpdatedAtUtc { get; set; }
}
