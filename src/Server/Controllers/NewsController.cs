using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NetAppForVika.Server.Data;
using NetAppForVika.Server.Models;
using NetAppForVika.Shared.Dtos;

namespace NetAppForVika.Server.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class NewsController : ControllerBase
{
    private readonly AppDbContext _dbContext;
    private readonly UserManager<AppUser> _userManager;

    public NewsController(AppDbContext dbContext, UserManager<AppUser> userManager)
    {
        _dbContext = dbContext;
        _userManager = userManager;
    }

    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<NewsArticleSummaryDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetArticles(CancellationToken cancellationToken)
    {
        var articles = await _dbContext.NewsArticles
            .AsNoTracking()
            .OrderByDescending(x => x.PublishedAtUtc)
            .Select(x => new NewsArticleSummaryDto(
                x.Id,
                x.Title,
                x.Slug,
                x.Excerpt,
                x.CoverImageUrl,
                x.AuthorName,
                x.PublishedAtUtc))
            .ToListAsync(cancellationToken);

        return Ok(articles);
    }

    [HttpGet("{slug}")]
    [ProducesResponseType(typeof(NewsArticleDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetArticleBySlug(string slug, CancellationToken cancellationToken)
    {
        var entity = await _dbContext.NewsArticles
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Slug == slug, cancellationToken);

        if (entity is null)
        {
            return NotFound();
        }

        var dto = new NewsArticleDto(
            entity.Id,
            entity.Title,
            entity.Slug,
            entity.Excerpt,
            entity.Content,
            entity.CoverImageUrl,
            entity.AuthorName,
            entity.PublishedAtUtc,
            entity.UpdatedAtUtc);

        return Ok(dto);
    }

    [Authorize(Roles = "Admin")]
    [HttpPost]
    [ProducesResponseType(typeof(NewsArticleDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> CreateArticle([FromBody] NewsArticleCreateDto request, CancellationToken cancellationToken)
    {
        if (await _dbContext.NewsArticles.AnyAsync(x => x.Slug == request.Slug, cancellationToken))
        {
            return Conflict("Slug already exists.");
        }

        var user = await _userManager.GetUserAsync(User);
        if (user is null)
        {
            return Forbid();
        }

        var now = DateTimeOffset.UtcNow;
        var entity = new NewsArticle
        {
            Id = Guid.NewGuid(),
            Title = request.Title,
            Slug = request.Slug,
            Excerpt = request.Excerpt,
            Content = request.Content,
            CoverImageUrl = request.CoverImageUrl,
            AuthorId = user.Id,
            AuthorName = user.DisplayName,
            PublishedAtUtc = now,
            UpdatedAtUtc = now
        };

        _dbContext.NewsArticles.Add(entity);
        await _dbContext.SaveChangesAsync(cancellationToken);

        var dto = new NewsArticleDto(
            entity.Id,
            entity.Title,
            entity.Slug,
            entity.Excerpt,
            entity.Content,
            entity.CoverImageUrl,
            entity.AuthorName,
            entity.PublishedAtUtc,
            entity.UpdatedAtUtc);

        return CreatedAtAction(nameof(GetArticleBySlug), new { slug = dto.Slug }, dto);
    }
}
