// Summary projection for listing news articles.
namespace NetAppForVika.Shared.Dtos;

/// <summary>
/// Lightweight projection used for the public news feed.
/// </summary>
public sealed record NewsArticleSummaryDto(
    Guid Id,
    string Title,
    string Slug,
    string Excerpt,
    string? CoverImageUrl,
    string AuthorName,
    DateTimeOffset PublishedAtUtc);
