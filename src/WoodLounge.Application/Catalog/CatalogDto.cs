namespace WoodLounge.Application.Catalog;

public record ProductDto
{
    public Guid Id { get; init; }
    public string Slug { get; init; } = null!;
    public string? Sku { get; init; }
    public string Name { get; init; } = null!;
    public string? ShortDescription { get; init; }
    public string? Description { get; init; }
    public Guid? CategoryId { get; init; }
    public string? CategoryName { get; init; }
    public long BasePriceKobo { get; init; }
    public long? CompareAtPriceKobo { get; init; }
    public string? Material { get; init; }
    public string? DimensionsText { get; init; }
    public decimal? WidthCm { get; init; }
    public decimal? DepthCm { get; init; }
    public decimal? HeightCm { get; init; }
    public bool IsMadeToOrder { get; init; }
    public int? LeadTimeDays { get; init; }
    public int? StockQty { get; init; }
    public bool IsBestseller { get; init; }
    public bool IsFeatured { get; init; }
    public bool IsPlaceholder { get; init; }
    public string? Badge { get; init; }
    public string Status { get; init; } = null!;
    public List<ProductImageDto> Images { get; init; } = new();
    public List<ProductOptionGroupDto> OptionGroups { get; init; } = new();
    public List<string> RoomSlugs { get; init; } = new();
    public List<ProductDto> RelatedProducts { get; init; } = new();
}

public record ProductImageDto
{
    public Guid Id { get; init; }
    public string Url { get; init; } = null!;
    public string? Alt { get; init; }
    public string Kind { get; init; } = null!;
    public bool IsPrimary { get; init; }
    public int SortOrder { get; init; }
}

public record ProductOptionGroupDto
{
    public Guid Id { get; init; }
    public string Name { get; init; } = null!;
    public int StepOrder { get; init; }
    public string InputType { get; init; } = null!;
    public bool IsRequired { get; init; }
    public List<ProductOptionValueDto> Values { get; init; } = new();
}

public record ProductOptionValueDto
{
    public Guid Id { get; init; }
    public string Label { get; init; } = null!;
    public string? SwatchHex { get; init; }
    public string? ImageUrl { get; init; }
    public long PriceDeltaKobo { get; init; }
    public bool IsDefault { get; init; }
    public int SortOrder { get; init; }
}

public record CategoryDto
{
    public Guid Id { get; init; }
    public string Slug { get; init; } = null!;
    public string Name { get; init; } = null!;
    public string? Description { get; init; }
    public string? ImageUrl { get; init; }
    public Guid? ParentId { get; init; }
    public int SortOrder { get; init; }
    public bool IsActive { get; init; }
    public List<CategoryDto> Children { get; init; } = new();
}

public record RoomDto
{
    public Guid Id { get; init; }
    public string Slug { get; init; } = null!;
    public string Name { get; init; } = null!;
    public string? ImageUrl { get; init; }
    public int SortOrder { get; init; }
}

public record DeliveryRateDto
{
    public string State { get; init; } = null!;
    public long FeeKobo { get; init; }
    public int EtaMinDays { get; init; }
    public int EtaMaxDays { get; init; }
    public bool IsActive { get; init; }
}

public record ShowroomDto
{
    public Guid Id { get; init; }
    public string Slug { get; init; } = null!;
    public string Name { get; init; } = null!;
    public string City { get; init; } = null!;
    public string State { get; init; } = null!;
    public string Address { get; init; } = null!;
    public string? Phone { get; init; }
    public string? Whatsapp { get; init; }
    public decimal? Latitude { get; init; }
    public decimal? Longitude { get; init; }
    public Dictionary<string, object> OpeningHours { get; init; } = new();
    public bool IsActive { get; init; }
}

public record PublicSettingsDto
{
    public string? WhatsappNumber { get; init; }
    public int DepositDefaultPercent { get; init; }
    public string? SupportEmail { get; init; }
}

public record ProductsQuery
{
    public string? Room { get; init; }
    public string? Category { get; init; }
    public string? Material { get; init; }
    public long? MinPrice { get; init; }
    public long? MaxPrice { get; init; }
    public bool? MadeToOrder { get; init; }
    public string? Q { get; init; }
    public string? Sort { get; init; }
    public bool? Bestseller { get; init; }
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 24;
}

public record PagedResult<T>
{
    public List<T> Items { get; init; } = new();
    public int Total { get; init; }
    public int Page { get; init; }
    public int PageSize { get; init; }
    public int TotalPages => (int)Math.Ceiling((double)Total / PageSize);
}
