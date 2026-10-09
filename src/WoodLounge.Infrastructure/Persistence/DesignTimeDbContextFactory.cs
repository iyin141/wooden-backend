using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace WoodLounge.Infrastructure.Persistence;

/// <summary>
/// Design-time factory for `dotnet ef migrations add` / `database update`.
/// Reads DATABASE_URL_DIRECT (direct host — required for migrations) or DATABASE_URL,
/// falling back to a local Postgres so migrations can be generated offline.
/// </summary>
public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<WoodLoungeDbContext>
{
    public WoodLoungeDbContext CreateDbContext(string[] args)
    {
        var conn = ConnectionStringNormalizer.Normalize(
            Environment.GetEnvironmentVariable("DATABASE_URL_DIRECT") ??
            Environment.GetEnvironmentVariable("DATABASE_URL") ??
            "Host=localhost;Port=5432;Database=woodlounge;Username=woodlounge;Password=woodlounge;SSL Mode=Disable");

        var options = new DbContextOptionsBuilder<WoodLoungeDbContext>()
            .UseNpgsql(conn)
            .UseSnakeCaseNamingConvention()
            .Options;

        return new WoodLoungeDbContext(options);
    }
}
