using Serilog;
using WoodLounge.Infrastructure;
using WoodLounge.Infrastructure.Persistence;
using WoodLounge.Infrastructure.Seeding;
using WoodLounge.Application;
using Microsoft.EntityFrameworkCore;
using InfraDI = WoodLounge.Infrastructure.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);

// ---- Serilog (structured, correlation id per request) ----
builder.Host.UseSerilog((ctx, lc) => lc
    .ReadFrom.Configuration(ctx.Configuration)
    .Enrich.FromLogContext()
    .Enrich.WithProperty("Application", "WoodLounge.Api")
    .WriteTo.Console());

// ---- Infrastructure (EF Core + Npgsql, hasher, seeder) ----
builder.Services.AddInfrastructure(builder.Configuration);

// ---- Application (services, use cases) ----
builder.Services.AddApplication();

// ---- Controllers + OpenAPI ----
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOpenApi();

// ---- CORS limited to configured web origins ----
var origins = (builder.Configuration["ALLOWED_ORIGINS"] ?? "http://localhost:3000")
    .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
builder.Services.AddCors(o => o.AddDefaultPolicy(p => p
    .WithOrigins(origins)
    .AllowCredentials()
    .AllowAnyHeader()
    .AllowAnyMethod()));

// ---- Health checks (db check for /healthz, /readyz) ----
builder.Services.AddHealthChecks()
    .AddDbContextCheck<WoodLoungeDbContext>("database");

var app = builder.Build();

SeedOptions SeedOptions() => InfraDI.GetSeedOptions(builder.Configuration, app.Environment.IsDevelopment());

// ---- CLI modes: --migrate, --seed (used by scripts + non-Development startup) ----
var wantMigrate = args.Contains("--migrate");
var wantSeed = args.Contains("--seed");
if (wantMigrate || wantSeed)
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<WoodLoungeDbContext>();
    if (wantMigrate)
    {
        app.Logger.LogInformation("Applying migrations…");
        await db.Database.MigrateAsync();
    }
    if (wantSeed)
    {
        var seeder = scope.ServiceProvider.GetRequiredService<DbSeeder>();
        await seeder.SeedAsync(SeedOptions());
    }
    return;
}

app.UseSerilogRequestLogging();
app.UseCors();
app.UseStaticFiles();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi(); // /openapi/v1.json
    // Auto-migrate + seed on startup in Development (spec §3.6).
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<WoodLoungeDbContext>();
    await db.Database.MigrateAsync();
    var seeder = scope.ServiceProvider.GetRequiredService<DbSeeder>();
    await seeder.SeedAsync(SeedOptions());
}

app.MapHealthChecks("/healthz");
app.MapHealthChecks("/readyz");

app.MapControllers();

app.Run();

/// <summary>Exposed for WebApplicationFactory in integration tests.</summary>
public partial class Program;
