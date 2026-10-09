using Microsoft.EntityFrameworkCore;
using WoodLounge.Application.Catalog;
using WoodLounge.Domain.Catalog;
using WoodLounge.Infrastructure.Persistence;

namespace WoodLounge.Infrastructure.Catalog;

public class CatalogRepository(WoodLoungeDbContext db) : Application.Catalog.ICatalogRepository
{
    public async Task<PagedResult<ProductDto>> GetProductsAsync(ProductsQuery query, CancellationToken ct = default)
    {
        var productsQuery = db.Products
            .Include(p => p.Category)
            .Include(p => p.Images.OrderBy(i => i.SortOrder))
            .Include(p => p.OptionGroups.OrderBy(g => g.StepOrder))
                .ThenInclude(g => g.Values.OrderBy(v => v.SortOrder))
            .Include(p => p.Rooms)
            .Where(p => p.Status == "active");

        // Filters
        if (!string.IsNullOrWhiteSpace(query.Room))
        {
            productsQuery = productsQuery.Where(p => p.Rooms.Any(r => r.Slug == query.Room));
        }

        if (!string.IsNullOrWhiteSpace(query.Category))
        {
            productsQuery = productsQuery.Where(p => p.Category != null && p.Category.Slug == query.Category);
        }

        if (!string.IsNullOrWhiteSpace(query.Material))
        {
            productsQuery = productsQuery.Where(p => p.Material != null && p.Material.ToLower().Contains(query.Material.ToLower()));
        }

        if (query.MinPrice.HasValue)
        {
            productsQuery = productsQuery.Where(p => p.BasePriceKobo >= query.MinPrice.Value);
        }

        if (query.MaxPrice.HasValue)
        {
            productsQuery = productsQuery.Where(p => p.BasePriceKobo <= query.MaxPrice.Value);
        }

        if (query.MadeToOrder.HasValue)
        {
            productsQuery = productsQuery.Where(p => p.IsMadeToOrder == query.MadeToOrder.Value);
        }

        if (!string.IsNullOrWhiteSpace(query.Q))
        {
            productsQuery = productsQuery.Where(p => p.Name.ToLower().Contains(query.Q.ToLower()));
        }

        if (query.Bestseller == true)
        {
            productsQuery = productsQuery.Where(p => p.IsBestseller);
        }

        // Sorting
        productsQuery = query.Sort?.ToLower() switch
        {
            "price-asc" => productsQuery.OrderBy(p => p.BasePriceKobo),
            "price-desc" => productsQuery.OrderByDescending(p => p.BasePriceKobo),
            "newest" => productsQuery.OrderByDescending(p => p.CreatedAt),
            _ => productsQuery.OrderBy(p => p.Name)
        };

        var total = await productsQuery.CountAsync(ct);

        var products = await productsQuery
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .ToListAsync(ct);

        var productDtos = products.Select(MapToProductDto).ToList();

        return new PagedResult<ProductDto>
        {
            Items = productDtos,
            Total = total,
            Page = query.Page,
            PageSize = query.PageSize
        };
    }

    public async Task<ProductDto?> GetProductBySlugAsync(string slug, CancellationToken ct = default)
    {
        var product = await db.Products
            .Include(p => p.Category)
            .Include(p => p.Images.OrderBy(i => i.SortOrder))
            .Include(p => p.OptionGroups.OrderBy(g => g.StepOrder))
                .ThenInclude(g => g.Values.OrderBy(v => v.SortOrder))
            .Include(p => p.Rooms)
            .FirstOrDefaultAsync(p => p.Slug == slug && p.Status == "active", ct);

        if (product == null) return null;

        var dto = MapToProductDto(product);

        // Load related products (same category, exclude self)
        if (product.CategoryId.HasValue)
        {
            var related = await db.Products
                .Include(p => p.Images.Where(i => i.IsPrimary).OrderBy(i => i.SortOrder))
                .Where(p => p.CategoryId == product.CategoryId && p.Id != product.Id && p.Status == "active")
                .OrderBy(p => p.IsBestseller ? 0 : 1)
                .ThenBy(p => p.CreatedAt)
                .Take(4)
                .ToListAsync(ct);

            dto = dto with { RelatedProducts = related.Select(MapToProductDto).ToList() };
        }

        return dto;
    }

    public async Task<ProductDto?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var product = await db.Products
            .Include(p => p.Category)
            .Include(p => p.Images.OrderBy(i => i.SortOrder))
            .FirstOrDefaultAsync(p => p.Id == id && p.Status == "active", ct);

        if (product == null) return null;

        return MapToProductDto(product);
    }

    public async Task<List<CategoryDto>> GetCategoriesAsync(CancellationToken ct = default)
    {
        var categories = await db.Categories
            .Include(c => c.Children.Where(child => child.IsActive))
            .Where(c => c.IsActive && c.ParentId == null)
            .OrderBy(c => c.SortOrder)
            .ToListAsync(ct);

        return categories.Select(MapToCategoryDto).ToList();
    }

    public async Task<List<RoomDto>> GetRoomsAsync(CancellationToken ct = default)
    {
        var rooms = await db.Rooms
            .OrderBy(r => r.SortOrder)
            .ToListAsync(ct);

        return rooms.Select(r => new RoomDto
        {
            Id = r.Id,
            Slug = r.Slug,
            Name = r.Name,
            ImageUrl = r.ImageUrl,
            SortOrder = r.SortOrder
        }).ToList();
    }

    public async Task<List<ShowroomDto>> GetShowroomsAsync(CancellationToken ct = default)
    {
        var showrooms = await db.Showrooms
            .Where(s => s.IsActive)
            .ToListAsync(ct);

        return showrooms.Select(s => new ShowroomDto
        {
            Id = s.Id,
            Slug = s.Slug,
            Name = s.Name,
            City = s.City,
            State = s.State,
            Address = s.Address,
            Phone = s.Phone,
            Whatsapp = s.Whatsapp,
            Latitude = s.Latitude,
            Longitude = s.Longitude,
            OpeningHours = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, object>>(s.OpeningHoursJson) ?? new(),
            IsActive = s.IsActive
        }).ToList();
    }

    public async Task<DeliveryRateDto?> GetDeliveryRateAsync(string state, CancellationToken ct = default)
    {
        var rate = await db.DeliveryRates
            .FirstOrDefaultAsync(r => r.State == state && r.IsActive, ct);

        if (rate == null) return null;

        return new DeliveryRateDto
        {
            State = rate.State,
            FeeKobo = rate.FeeKobo,
            EtaMinDays = rate.EtaMinDays,
            EtaMaxDays = rate.EtaMaxDays,
            IsActive = rate.IsActive
        };
    }

    public async Task<List<DeliveryRateDto>> GetDeliveryRatesAsync(CancellationToken ct = default)
    {
        var rates = await db.DeliveryRates
            .Where(r => r.IsActive)
            .OrderBy(r => r.State)
            .ToListAsync(ct);

        return rates.Select(r => new DeliveryRateDto
        {
            State = r.State,
            FeeKobo = r.FeeKobo,
            EtaMinDays = r.EtaMinDays,
            EtaMaxDays = r.EtaMaxDays,
            IsActive = r.IsActive
        }).ToList();
    }

    public async Task<PublicSettingsDto> GetPublicSettingsAsync(CancellationToken ct = default)
    {
        var settings = await db.SiteSettings
            .ToDictionaryAsync(s => s.Key, s => s.ValueJson, ct);

        var whatsappNumber = settings.TryGetValue("whatsapp_number", out var whatsappValue)
            ? System.Text.Json.JsonDocument.Parse(whatsappValue).RootElement.GetProperty("value").GetString()
            : null;

        var depositPercent = settings.TryGetValue("deposit_default_percent", out var depositValue)
            ? System.Text.Json.JsonDocument.Parse(depositValue).RootElement.GetProperty("value").GetInt32()
            : 50;

        var supportEmail = settings.TryGetValue("support_email", out var emailValue)
            ? System.Text.Json.JsonDocument.Parse(emailValue).RootElement.GetProperty("value").GetString()
            : null;

        return new PublicSettingsDto
        {
            WhatsappNumber = whatsappNumber,
            DepositDefaultPercent = depositPercent,
            SupportEmail = supportEmail
        };
    }

    private static ProductDto MapToProductDto(Product product)
    {
        return new ProductDto
        {
            Id = product.Id,
            Slug = product.Slug,
            Sku = product.Sku,
            Name = product.Name,
            ShortDescription = product.ShortDescription,
            Description = product.Description,
            CategoryId = product.CategoryId,
            CategoryName = product.Category?.Name,
            BasePriceKobo = product.BasePriceKobo,
            CompareAtPriceKobo = product.CompareAtPriceKobo,
            Material = product.Material,
            DimensionsText = product.DimensionsText,
            WidthCm = product.WidthCm,
            DepthCm = product.DepthCm,
            HeightCm = product.HeightCm,
            IsMadeToOrder = product.IsMadeToOrder,
            LeadTimeDays = product.LeadTimeDays,
            StockQty = product.StockQty,
            IsBestseller = product.IsBestseller,
            IsFeatured = product.IsFeatured,
            IsPlaceholder = product.IsPlaceholder,
            Badge = product.Badge,
            Status = product.Status,
            Images = product.Images.Select(i => new ProductImageDto
            {
                Id = i.Id,
                Url = i.Url,
                Alt = i.Alt,
                Kind = i.Kind,
                IsPrimary = i.IsPrimary,
                SortOrder = i.SortOrder
            }).ToList(),
            OptionGroups = product.OptionGroups.Select(g => new ProductOptionGroupDto
            {
                Id = g.Id,
                Name = g.Name,
                StepOrder = g.StepOrder,
                InputType = g.InputType,
                IsRequired = g.IsRequired,
                Values = g.Values.Select(v => new ProductOptionValueDto
                {
                    Id = v.Id,
                    Label = v.Label,
                    SwatchHex = v.SwatchHex,
                    ImageUrl = v.ImageUrl,
                    PriceDeltaKobo = v.PriceDeltaKobo,
                    IsDefault = v.IsDefault,
                    SortOrder = v.SortOrder
                }).ToList()
            }).ToList(),
            RoomSlugs = product.Rooms.Select(r => r.Slug).ToList()
        };
    }

    private static CategoryDto MapToCategoryDto(Category category)
    {
        return new CategoryDto
        {
            Id = category.Id,
            Slug = category.Slug,
            Name = category.Name,
            Description = category.Description,
            ImageUrl = category.ImageUrl,
            ParentId = category.ParentId,
            SortOrder = category.SortOrder,
            IsActive = category.IsActive,
            Children = category.Children
                .Where(c => c.IsActive)
                .Select(MapToCategoryDto)
                .OrderBy(c => c.SortOrder)
                .ToList()
        };
    }
}
