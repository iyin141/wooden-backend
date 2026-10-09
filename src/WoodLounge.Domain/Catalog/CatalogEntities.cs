using WoodLounge.Domain.Common;

namespace WoodLounge.Domain.Catalog;

public class Category : Entity
{
    public string Slug { get; set; } = null!;       // citext unique
    public string Name { get; set; } = null!;
    public string? Description { get; set; }
    public string? ImageUrl { get; set; }
    public Guid? ParentId { get; set; }
    public Category? Parent { get; set; }
    public int SortOrder { get; set; }
    public bool IsActive { get; set; } = true;

    public ICollection<Category> Children { get; set; } = new List<Category>();
    public ICollection<Product> Products { get; set; } = new List<Product>();
}

public class Room : Entity
{
    public string Slug { get; set; } = null!;       // citext unique
    public string Name { get; set; } = null!;
    public string? ImageUrl { get; set; }
    public int SortOrder { get; set; }

    public ICollection<Product> Products { get; set; } = new List<Product>();
}

public class Product : AuditedEntity
{
    public string Slug { get; set; } = null!;       // citext unique
    public string? Sku { get; set; }
    public string Name { get; set; } = null!;
    public string? ShortDescription { get; set; }
    public string? Description { get; set; }
    public Guid? CategoryId { get; set; }
    public Category? Category { get; set; }
    public long BasePriceKobo { get; set; }
    public long? CompareAtPriceKobo { get; set; }
    public string? Material { get; set; }
    public string? DimensionsText { get; set; }
    public decimal? WidthCm { get; set; }           // numeric(8,2)
    public decimal? DepthCm { get; set; }
    public decimal? HeightCm { get; set; }
    public bool IsMadeToOrder { get; set; }
    public int? LeadTimeDays { get; set; }
    public int? StockQty { get; set; }              // null = not tracked
    public bool IsBestseller { get; set; }
    public bool IsFeatured { get; set; }
    public bool IsPlaceholder { get; set; }         // mock content flagged for replacement
    public string? Badge { get; set; }              // e.g. 'New', 'Bestseller'
    public string Status { get; set; } = "draft";   // draft | active | archived

    public ICollection<Room> Rooms { get; set; } = new List<Room>();
    public ICollection<ProductImage> Images { get; set; } = new List<ProductImage>();
    public ICollection<ProductOptionGroup> OptionGroups { get; set; } = new List<ProductOptionGroup>();
}

public class ProductImage : Entity
{
    public Guid ProductId { get; set; }
    public Product? Product { get; set; }
    public string Url { get; set; } = null!;
    public string? Alt { get; set; }
    public string Kind { get; set; } = "studio";    // studio | lifestyle | customer_home
    public bool IsPrimary { get; set; }
    public int SortOrder { get; set; }
}

public class ProductOptionGroup : Entity
{
    public Guid ProductId { get; set; }
    public Product? Product { get; set; }
    public string Name { get; set; } = null!;
    public int StepOrder { get; set; }
    public string InputType { get; set; } = "select"; // select | swatch | radio
    public bool IsRequired { get; set; } = true;

    public ICollection<ProductOptionValue> Values { get; set; } = new List<ProductOptionValue>();
}

public class ProductOptionValue : Entity
{
    public Guid GroupId { get; set; }
    public ProductOptionGroup? Group { get; set; }
    public string Label { get; set; } = null!;
    public string? SwatchHex { get; set; }
    public string? ImageUrl { get; set; }
    public long PriceDeltaKobo { get; set; }
    public bool IsDefault { get; set; }
    public int SortOrder { get; set; }
}

public class DeliveryRate : Entity
{
    public string State { get; set; } = null!;      // unique
    public long FeeKobo { get; set; }
    public int EtaMinDays { get; set; }
    public int EtaMaxDays { get; set; }
    public bool IsActive { get; set; } = true;
}

public class Showroom : Entity
{
    public string Slug { get; set; } = null!;       // citext unique
    public string Name { get; set; } = null!;
    public string City { get; set; } = null!;
    public string State { get; set; } = null!;
    public string Address { get; set; } = null!;
    public string? Phone { get; set; }
    public string? Whatsapp { get; set; }           // placeholder until client supplies it
    public decimal? Latitude { get; set; }          // numeric(9,6)
    public decimal? Longitude { get; set; }
    public string OpeningHoursJson { get; set; } = "{}"; // jsonb
    public bool IsActive { get; set; } = true;
}
