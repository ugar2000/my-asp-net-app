// DTO representing a rich news article suitable for detail views.
namespace NetAppForVika.Shared.Dtos;

/// <summary>
/// Serialized news article.
/// </summary>
public sealed record NewsArticleDto(
    Guid Id,
    string Title,
    string Slug,
    string Excerpt,
    string Content,
    string? CoverImageUrl,
    string AuthorName,
    DateTimeOffset PublishedAtUtc,
    DateTimeOffset UpdatedAtUtc);
