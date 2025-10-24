// Design-time factory enabling EF Core CLI migrations without touching Program.cs.
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace NetAppForVika.Server.Data;

/// <summary>
/// Provides an EF Core context factory for tooling scenarios such as migrations.
/// </summary>
public sealed class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    /// <summary>
    /// Builds a context using environment variables or sensible defaults for design-time usage.
    /// </summary>
    public AppDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();
        var connectionString =
            Environment.GetEnvironmentVariable("DATABASE_URL") ??
            "Host=localhost;Database=net_app_for_vika;Username=postgres;Password=postgres";

        optionsBuilder.UseNpgsql(connectionString);
        return new AppDbContext(optionsBuilder.Options);
    }
}
