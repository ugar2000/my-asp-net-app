// Central EF Core context tying PostgreSQL tables to the domain entities used across the API surface.
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using NetAppForVika.Server.Models;

namespace NetAppForVika.Server.Data;

/// <summary>
/// Database context that stores analytic summaries and collaborative session snapshots.
/// </summary>
public sealed class AppDbContext : IdentityDbContext<AppUser, AppRole, string>
{
    /// <summary>
    /// Provides DI-friendly constructor so Entity Framework can inject configured options.
    /// </summary>
    /// <param name="options">Database options built via Program.cs.</param>
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    /// <summary>
    /// Table of algorithm runs aggregated for the analytics dashboards.
    /// </summary>
    public DbSet<AlgorithmRun> AlgorithmRuns => Set<AlgorithmRun>();

    /// <summary>
    /// Table storing collaborative lesson state for recovery if Redis evicts entries.
    /// </summary>
    public DbSet<ClubSessionSnapshot> ClubSessions => Set<ClubSessionSnapshot>();

    /// <summary>
    /// News articles authored by admins.
    /// </summary>
    public DbSet<NewsArticle> NewsArticles => Set<NewsArticle>();

    /// <summary>
    /// Configures schema details such as indexes so LINQ queries stay efficient.
    /// </summary>
    /// <param name="modelBuilder">Fluent builder that manages entity metadata.</param>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<AlgorithmRun>(entity =>
        {
            entity.HasIndex(x => new { x.Family, x.AlgorithmName });
            entity.HasIndex(x => x.CreatedAtUtc);
        });

        modelBuilder.Entity<ClubSessionSnapshot>(entity =>
        {
            entity.HasIndex(x => x.SessionId).IsUnique();
        });

        modelBuilder.Entity<NewsArticle>(entity =>
        {
            entity.HasIndex(x => x.Slug).IsUnique();
            entity.Property(x => x.PublishedAtUtc).HasDefaultValueSql("NOW()");
        });
    }
}
