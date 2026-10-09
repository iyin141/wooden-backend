namespace WoodLounge.Infrastructure.Seeding;

/// <summary>
/// Nigeria's 36 states + the FCT. Used to seed delivery_rates (spec §4 seed).
/// Fees/ETAs are placeholders — see docs/NEEDS_DECISION.md.
/// </summary>
public static class NigerianStates
{
    public static readonly string[] All =
    {
        "Abia", "Adamawa", "Akwa Ibom", "Anambra", "Bauchi", "Bayelsa", "Benue", "Borno",
        "Cross River", "Delta", "Ebonyi", "Edo", "Ekiti", "Enugu", "Gombe", "Imo", "Jigawa",
        "Kaduna", "Kano", "Katsina", "Kebbi", "Kogi", "Kwara", "Lagos", "Nasarawa", "Niger",
        "Ogun", "Ondo", "Osun", "Oyo", "Plateau", "Rivers", "Sokoto", "Taraba", "Yobe", "Zamfara",
        "Federal Capital Territory",
    };

    /// <summary>Base hub states get the cheapest fee / fastest ETA (Enugu + Abuja showrooms).</summary>
    private static readonly HashSet<string> HubStates = new() { "Enugu", "Federal Capital Territory" };
    private static readonly HashSet<string> NearStates = new()
    {
        "Anambra", "Ebonyi", "Abia", "Imo", "Rivers", "Delta", "Benue", "Kogi", "Kwara",
        "Lagos", "Ogun", "Nasarawa", "Kaduna", "Plateau",
    };

    /// <summary>Placeholder fee in kobo. MUST be replaced with the real table (NEEDS_DECISION).</summary>
    public static long PlaceholderFeeKobo(string state)
        => HubStates.Contains(state) ? 15_000
         : NearStates.Contains(state) ? 35_000
         : 55_000;

    public static (int min, int max) PlaceholderEtaDays(string state)
        => HubStates.Contains(state) ? (1, 3)
         : NearStates.Contains(state) ? (3, 6)
         : (5, 10);
}
