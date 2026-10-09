namespace WoodLounge.Infrastructure.Seeding;

/// <summary>Options controlling the seed run (populated from env / configuration).</summary>
public class SeedOptions
{
    public bool IsDevelopment { get; init; }
    public bool SeedDemoData { get; init; }
    public string? SeedAdminEmail { get; init; }
    public string? SeedAdminPassword { get; init; }
}
