using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using WoodLounge.Application.Common.Security;
using WoodLounge.Domain.Catalog;
using WoodLounge.Domain.CustomOrders;
using WoodLounge.Domain.Identity;
using WoodLounge.Domain.Messaging;
using WoodLounge.Domain.Operations;
using WoodLounge.Domain.Orders;
using WoodLounge.Domain.Payments;
using WoodLounge.Domain.Visits;
using WoodLounge.Infrastructure.Persistence;

namespace WoodLounge.Infrastructure.Seeding;

/// <summary>
/// Idempotent seed (spec §4 "Seed data"). Safe to run repeatedly: every section checks for
/// existing rows by natural key before inserting. Run on startup in Development and via CLI
/// flag elsewhere. Demo data (customers/visits/requests/quotes/orders/payments) is Development-only.
/// </summary>
public partial class DbSeeder(
    WoodLoungeDbContext db,
    IPasswordHasher passwords,
    ILogger<DbSeeder> log)
{
    public async Task SeedAsync(SeedOptions options, CancellationToken ct = default)
    {
        log.LogInformation("Seeding database (idempotent)…");

        await SeedDeliveryRatesAsync(ct);
        await SeedShowroomsAsync(ct);
        await SeedRoomsAndCategoriesAsync(ct);
        var products = await SeedProductsAsync(ct);
        await SeedVisitAvailabilityAsync(ct);
        await SeedSiteSettingsAsync(ct);
        await SeedSuperAdminAsync(options, ct);

        if (options.SeedDemoData)
        {
            await SeedDemoDataAsync(products, ct);
        }

        await db.SaveChangesAsync(ct);
        log.LogInformation("Seed complete.");
    }

    // ---- delivery_rates: 36 states + FCT (placeholder fees) ----
    private async Task SeedDeliveryRatesAsync(CancellationToken ct)
    {
        var existing = await db.DeliveryRates.Select(x => x.State).ToListAsync(ct);
        var have = existing.ToHashSet(StringComparer.OrdinalIgnoreCase);
        foreach (var state in NigerianStates.All)
        {
            if (have.Contains(state)) continue;
            var (min, max) = NigerianStates.PlaceholderEtaDays(state);
            db.DeliveryRates.Add(new DeliveryRate
            {
                State = state,
                FeeKobo = NigerianStates.PlaceholderFeeKobo(state),
                EtaMinDays = min,
                EtaMaxDays = max,
                IsActive = true,
            });
        }
    }

    // ---- showrooms: Enugu (main) + Abuja, from the static site's locationsData ----
    private async Task SeedShowroomsAsync(CancellationToken ct)
    {
        if (await db.Showrooms.AnyAsync(ct)) return;
        db.Showrooms.AddRange(
            new Showroom
            {
                Slug = "enugu",
                Name = "Enugu Showroom — Asata Chemist",
                City = "Enugu",
                State = "Enugu",
                Address = "160 Ogui Rd, Asata Chemist, Enugu, Nigeria",
                Phone = null,
                Whatsapp = "+234XXXXXXXXXX", // placeholder (NEEDS_DECISION)
                Latitude = 6.447400m,
                Longitude = 7.513900m,
                OpeningHoursJson = """{"mon_sat":"09:00-17:00","sun":"closed"}""",
                IsActive = true,
            },
            new Showroom
            {
                Slug = "abuja",
                Name = "Abuja Showroom — Wuse 2",
                City = "Abuja",
                State = "Federal Capital Territory",
                Address = "2 Lobito Crescent, Wuse 2, Abuja, Nigeria",
                Phone = null,
                Whatsapp = "+234XXXXXXXXXX", // placeholder (NEEDS_DECISION)
                Latitude = 9.076500m,
                Longitude = 7.498100m,
                OpeningHoursJson = """{"mon_sat":"09:00-17:00","sun":"closed"}""",
                IsActive = true,
            });
    }

    // ---- rooms + categories ----
    private async Task SeedRoomsAndCategoriesAsync(CancellationToken ct)
    {
        if (!await db.Rooms.AnyAsync(ct))
        {
            var rooms = new (string slug, string name, int sort)[]
            {
                ("lounge", "Lounge", 1),
                ("living", "Living Room", 2),
                ("dining", "Dining", 3),
                ("bedroom", "Bedroom", 4),
                ("office", "Office", 5),
            };
            foreach (var (slug, name, sort) in rooms)
                db.Rooms.Add(new Room { Slug = slug, Name = name, SortOrder = sort });
        }

        if (!await db.Categories.AnyAsync(ct))
        {
            var cats = new (string slug, string name, int sort)[]
            {
                ("sofas", "Sofas & Lounges", 1),
                ("chairs", "Accent Chairs", 2),
                ("tables", "Tables", 3),
                ("storage", "Credenzas & Storage", 4),
                ("benches", "Benches & Consoles", 5),
                ("bedding", "Bedding", 6), // placeholder category for the mock bestsellers
            };
            foreach (var (slug, name, sort) in cats)
                db.Categories.Add(new Category { Slug = slug, Name = name, SortOrder = sort, IsActive = true });
        }
        await db.SaveChangesAsync(ct);
    }

    // ---- products: every product visible in the mockup, incl. bedding bestsellers (is_placeholder) ----
    private async Task<Dictionary<string, Product>> SeedProductsAsync(CancellationToken ct)
    {
        await db.SaveChangesAsync(ct); // ensure rooms/categories persisted for FK lookups
        var rooms = await db.Rooms.ToDictionaryAsync(r => r.Slug, ct);
        var cats = await db.Categories.ToDictionaryAsync(c => c.Slug, ct);

        var existing = await db.Products.Include(p => p.Images).Include(p => p.OptionGroups)
            .ThenInclude(g => g.Values).ToListAsync(ct);
        var bySlug = existing.ToDictionary(p => p.Slug, StringComparer.OrdinalIgnoreCase);

        // Real furniture products (canonical prices — see NEEDS_DECISION "Canonical prices")
        var specs = new ProductSpec[]
        {
            new("nsukka-curved-sofa", "WL-SF-NSK-240", "Nsukka Curved Sofa", "sofas", ["lounge","living"],
                229_000, "Curved 3-seater sofa, hand-joined hardwood frame.", "Solid Wood", false, 4, "Bestseller", true),
            new("ogui-modular-sofa", "WL-SF-OGU-320", "Ogui Modular Sofa", "sofas", ["lounge","living"],
                680_000, "Modular sectional sofa in solid hardwood.", "Solid Wood", false, 2, null, false),
            new("lobito-accent-chair", "WL-AC-LOB-014", "Lobito Accent Chair", "chairs", ["living"],
                285_000, "Sculptural accent chair.", "Solid Wood", false, 2, null, false),
            new("asata-coffee-table", "WL-TB-AST-110", "Asata Coffee Table", "tables", ["living","lounge"],
                229_000, "Low coffee table in kiln-dried hardwood.", "Solid Wood", false, 6, null, false),
            new("wuse-credenza", "WL-CR-WUS-091", "Wuse Credenza", "storage", ["living","office"],
                540_000, "Wide credenza with brass hardware.", "Solid Wood", true, null, null, false),
            new("aroma-side-table", null, "Aroma Side Table", "tables", ["living"],
                340_000, "Compact side table.", "Solid Wood", false, 3, null, false),
            new("zik-console", "WL-CS-ZIK-005", "Zik Console", "benches", ["living"],
                195_000, "Slim entry console.", "Solid Wood", false, 3, null, false),
            new("independence-bench", null, "Independence Bench", "benches", ["living","dining"],
                310_000, "Solid hardwood bench.", "Solid Wood", false, 2, null, false),
            new("capitol-armchair", null, "Capitol Armchair", "chairs", ["office","living"],
                475_000, "Upholstered armchair with timber frame.", "Solid Wood", false, 2, null, false),
            new("executive-wall-credenza", "WL-CR-EXC-082", "Executive Wall Credenza", "storage", ["office"],
                300_000, "Bespoke wall credenza (base price).", "Cross River Walnut", true, null, null, false),
            new("fluted-dining-table-10-seater", "WL-DT-FLU-10S", "Fluted Dining Table 10-Seater", "tables", ["dining"],
                1_800_000, "Ten-seater fluted dining table.", "Solid Wood", true, null, null, false),
        };

        // Bedding bestsellers — intentional mock content (NEEDS_DECISION: MUST-REPLACE before external pitch)
        var bedding = new ProductSpec[]
        {
            new("signature-linen-curved-bed-set", null, "Signature Linen Curved Bed Set", "bedding", ["bedroom"],
                229_000, "Signature linen bed set.", "Linen", false, null, "Bestseller", true, Placeholder: true),
            new("waffle-weave-accent-throw", null, "Waffle Weave Accent Throw", "bedding", ["bedroom"],
                239_000, "Waffle weave throw.", "Cotton", false, null, "Apartment Therapy", true, Placeholder: true),
            new("curved-espresso-lounge-bedding", null, "Curved Espresso Lounge Bedding", "bedding", ["bedroom"],
                229_000, "Espresso lounge bedding.", "Linen", false, null, "Apartment Therapy", true, Placeholder: true),
            new("minimalist-duo-percale-suite", null, "Minimalist Duo Percale Suite", "bedding", ["bedroom"],
                458_000, "Percale suite.", "Percale", false, null, "Award", true, Placeholder: true),
            new("botanical-luxury-comforter-ensemble", null, "Botanical Luxury Comforter Ensemble", "bedding", ["bedroom"],
                439_000, "Botanical comforter ensemble.", "Cotton", false, null, null, true, Placeholder: true),
        };

        foreach (var spec in specs.Concat(bedding))
        {
            if (bySlug.TryGetValue(spec.Slug, out var existingProduct))
            {
                bySlug[spec.Slug] = existingProduct;
                continue;
            }
            var product = new Product
            {
                Slug = spec.Slug,
                Sku = spec.Sku,
                Name = spec.Name,
                ShortDescription = spec.Description,
                Description = spec.Description,
                CategoryId = cats.TryGetValue(spec.CategorySlug, out var cat) ? cat.Id : null,
                BasePriceKobo = spec.PriceKobo,
                Material = spec.Material,
                IsMadeToOrder = spec.MadeToOrder,
                StockQty = spec.Stock,
                IsBestseller = spec.Bestseller,
                IsPlaceholder = spec.Placeholder,
                Badge = spec.Badge,
                Status = "active",
            };
            foreach (var roomSlug in spec.RoomSlugs)
                if (rooms.TryGetValue(roomSlug, out var room)) product.Rooms.Add(room);

            product.Images.Add(new ProductImage
            {
                Url = $"/images/products/{spec.Slug}.webp", // placeholder until Stitch images are mirrored
                Alt = spec.Name,
                Kind = "studio",
                IsPrimary = true,
                SortOrder = 0,
            });

            db.Products.Add(product);
            bySlug[spec.Slug] = product;
        }

        await db.SaveChangesAsync(ct);

        // Option groups/values for the Nsukka sofa (size / wood / finish steps from the PDP)
        var nsukka = bySlug["nsukka-curved-sofa"];
        if (!nsukka.OptionGroups.Any())
        {
            nsukka.OptionGroups.Add(new ProductOptionGroup
            {
                Name = "Size", StepOrder = 1, InputType = "radio", IsRequired = true,
                Values =
                {
                    new ProductOptionValue { Label = "2-Seater", PriceDeltaKobo = -31_000, SortOrder = 0 },
                    new ProductOptionValue { Label = "3-Seater", PriceDeltaKobo = 0, IsDefault = true, SortOrder = 1 },
                    new ProductOptionValue { Label = "Sectional", PriceDeltaKobo = 116_000, SortOrder = 2 },
                },
            });
            nsukka.OptionGroups.Add(new ProductOptionGroup
            {
                Name = "Wood", StepOrder = 2, InputType = "swatch", IsRequired = true,
                Values =
                {
                    new ProductOptionValue { Label = "Teak", SwatchHex = "#633a21", PriceDeltaKobo = 0, IsDefault = true, SortOrder = 0 },
                    new ProductOptionValue { Label = "Iroko", SwatchHex = "#462e1e", PriceDeltaKobo = 35_000, SortOrder = 1 },
                },
            });
            nsukka.OptionGroups.Add(new ProductOptionGroup
            {
                Name = "Finish", StepOrder = 3, InputType = "swatch", IsRequired = true,
                Values =
                {
                    new ProductOptionValue { Label = "Oatmeal Sand Bouclé", SwatchHex = "#dfd6c6", IsDefault = true, SortOrder = 0 },
                    new ProductOptionValue { Label = "Warm Umber", SwatchHex = "#7a482b", SortOrder = 1 },
                    new ProductOptionValue { Label = "Deep Forest Velvet", SwatchHex = "#2a3c2e", SortOrder = 2 },
                    new ProductOptionValue { Label = "Enugu Terracotta Linen", SwatchHex = "#964627", SortOrder = 3 },
                    new ProductOptionValue { Label = "Charcoal Slate Weave", SwatchHex = "#3a3a3a", SortOrder = 4 },
                },
            });
            await db.SaveChangesAsync(ct);
        }

        return bySlug;
    }

    // ---- visit_availability defaults: Mon–Sat, 09:00–17:00, 60-min slots ----
    private async Task SeedVisitAvailabilityAsync(CancellationToken ct)
    {
        if (await db.VisitAvailabilities.AnyAsync(ct)) return;
        var start = new TimeOnly(9, 0);
        var end = new TimeOnly(17, 0);
        foreach (var type in new[] { "site_inspection", "online_visit", "showroom_visit" })
            for (short day = 1; day <= 6; day++) // 0 = Sunday (skipped); Mon–Sat
                db.VisitAvailabilities.Add(new VisitAvailability
                {
                    VisitType = type,
                    Weekday = day,
                    StartTime = start,
                    EndTime = end,
                    SlotMinutes = 60,
                    Capacity = type == "online_visit" ? 3 : 1,
                    IsActive = true,
                });
    }

    // ---- site_settings: whatsapp number (placeholder) + default deposit percent ----
    private async Task SeedSiteSettingsAsync(CancellationToken ct)
    {
        var keys = await db.SiteSettings.Select(s => s.Key).ToListAsync(ct);
        var have = keys.ToHashSet(StringComparer.OrdinalIgnoreCase);
        if (!have.Contains("whatsapp_number"))
            db.SiteSettings.Add(new SiteSetting { Key = "whatsapp_number", ValueJson = "\"+234XXXXXXXXXX\"" });
        if (!have.Contains("deposit_default_percent"))
            db.SiteSettings.Add(new SiteSetting { Key = "deposit_default_percent", ValueJson = "50" });
        if (!have.Contains("support_email"))
            db.SiteSettings.Add(new SiteSetting { Key = "support_email", ValueJson = "\"hello@woodlounge.test\"" });
    }

    // ---- one super_admin from env; refuse default password outside Development ----
    private async Task SeedSuperAdminAsync(SeedOptions options, CancellationToken ct)
    {
        var email = options.SeedAdminEmail;
        var password = options.SeedAdminPassword;
        if (string.IsNullOrWhiteSpace(email))
        {
            log.LogWarning("SEED_ADMIN_EMAIL not set — skipping super_admin seed.");
            return;
        }
        if (await db.Users.AnyAsync(u => u.Email == email, ct)) return;

        if (!options.IsDevelopment &&
            (string.IsNullOrWhiteSpace(password) || password is "change-me-in-dev-only" or "password"))
        {
            throw new InvalidOperationException(
                "Refusing to seed a super_admin with a default/empty password outside Development. " +
                "Set SEED_ADMIN_PASSWORD to a strong value.");
        }
        if (string.IsNullOrWhiteSpace(password))
            throw new InvalidOperationException("SEED_ADMIN_PASSWORD is required to seed a super_admin.");

        db.Users.Add(new User
        {
            Email = email,
            FullName = "Workshop Admin",
            Role = "super_admin",
            PasswordHash = passwords.Hash(password),
            EmailVerifiedAt = DateTimeOffset.UtcNow,
            IsActive = true,
        });
    }

    private record ProductSpec(
        string Slug, string? Sku, string Name, string CategorySlug, string[] RoomSlugs,
        long PriceKobo, string Description, string Material, bool MadeToOrder, int? Stock,
        string? Badge, bool Bestseller, bool Placeholder = false);
}
