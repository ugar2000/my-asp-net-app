// Request payload for authoring a new article in the admin surface.
namespace NetAppForVika.Shared.Dtos;

/// <summary>
/// Admin-facing request to create or update a news article.
/// </summary>
public sealed class NewsArticleCreateDto
{
    public string Title { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string Excerpt { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public string? CoverImageUrl { get; set; }
}
