using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using WoodLounge.Application.Auth;
using WoodLounge.Application.Catalog;
using WoodLounge.Application.Common.Security;
using WoodLounge.Infrastructure.Auth;
using WoodLounge.Infrastructure.Catalog;
using WoodLounge.Infrastructure.Persistence;
using WoodLounge.Infrastructure.Security;
using WoodLounge.Infrastructure.Seeding;

namespace WoodLounge.Infrastructure;

public static class DependencyInjection
{
    /// <summary>
    /// Registers EF Core (Npgsql, snake_case naming, retry-on-failure for Neon scale-to-zero),
    /// the password hasher, the seeder, repositories, and JWT service.
    /// </summary>
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration config)
    {
        // Running API uses the pooled URL; migrations use the direct URL (see design-time factory).
        var conn = ConnectionStringNormalizer.Normalize(
            config.GetConnectionString("Database")
            ?? Environment.GetEnvironmentVariable("DATABASE_URL")
            ?? throw new InvalidOperationException(
                "No database connection configured. Set ConnectionStrings:Database or DATABASE_URL."));

        services.AddDbContext<WoodLoungeDbContext>((sp, options) =>
            options.UseNpgsql(conn, npgsql =>
                    npgsql.EnableRetryOnFailure(maxRetryCount: 5, maxRetryDelay: TimeSpan.FromSeconds(10), errorCodesToAdd: null)
                          .MigrationsAssembly(typeof(WoodLoungeDbContext).Assembly.FullName))
                .UseSnakeCaseNamingConvention());

        services.AddSingleton<IPasswordHasher, BCryptPasswordHasher>();
        services.AddScoped<DbSeeder>();
        services.AddScoped<WoodLounge.Application.Catalog.ICatalogRepository, CatalogRepository>();
        services.AddScoped<IAuthRepository, AuthRepository>();
        services.AddScoped<IJwtTokenService, JwtTokenService>();

        return services;
    }

    /// <summary>Builds SeedOptions from configuration/environment.</summary>
    public static SeedOptions GetSeedOptions(IConfiguration config, bool isDevelopment) => new()
    {
        IsDevelopment = isDevelopment,
        // Demo data only in Development unless explicitly enabled.
        SeedDemoData = isDevelopment ||
            string.Equals(config["Seed:DemoData"], "true", StringComparison.OrdinalIgnoreCase),
        SeedAdminEmail = config["SEED_ADMIN_EMAIL"] ?? Environment.GetEnvironmentVariable("SEED_ADMIN_EMAIL"),
        SeedAdminPassword = config["SEED_ADMIN_PASSWORD"] ?? Environment.GetEnvironmentVariable("SEED_ADMIN_PASSWORD"),
    };
}
