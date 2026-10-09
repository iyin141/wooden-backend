using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WoodLounge.Domain.Catalog;

namespace WoodLounge.Infrastructure.Persistence.Configurations;

public class CategoryConfiguration : IEntityTypeConfiguration<Category>
{
    public void Configure(EntityTypeBuilder<Category> b)
    {
        b.ToTable("categories");
        b.HasKey(x => x.Id);
        b.Property(x => x.Id).HasDefaultValueSql("gen_random_uuid()");
        b.Property(x => x.Slug).HasColumnType("citext").IsRequired();
        b.HasIndex(x => x.Slug).IsUnique();
        b.Property(x => x.Name).HasColumnType("text").IsRequired();
        b.Property(x => x.Description).HasColumnType("text");
        b.Property(x => x.ImageUrl).HasColumnType("text");
        b.Property(x => x.SortOrder).HasDefaultValue(0);
        b.Property(x => x.IsActive).HasDefaultValue(true);
        b.HasOne(x => x.Parent).WithMany(x => x.Children).HasForeignKey(x => x.ParentId).OnDelete(DeleteBehavior.NoAction);
    }
}

public class RoomConfiguration : IEntityTypeConfiguration<Room>
{
    public void Configure(EntityTypeBuilder<Room> b)
    {
        b.ToTable("rooms");
        b.HasKey(x => x.Id);
        b.Property(x => x.Id).HasDefaultValueSql("gen_random_uuid()");
        b.Property(x => x.Slug).HasColumnType("citext").IsRequired();
        b.HasIndex(x => x.Slug).IsUnique();
        b.Property(x => x.Name).HasColumnType("text").IsRequired();
        b.Property(x => x.ImageUrl).HasColumnType("text");
        b.Property(x => x.SortOrder).HasDefaultValue(0);
    }
}

public class ProductConfiguration : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> b)
    {
        b.ToTable("products", t =>
        {
            t.HasCheckConstraint("ck_products_base_price", "base_price_kobo >= 0");
            t.HasCheckConstraint("ck_products_compare_price", "compare_at_price_kobo is null or compare_at_price_kobo >= 0");
            t.HasCheckConstraint("ck_products_stock", "stock_qty is null or stock_qty >= 0");
            t.HasCheckConstraint("ck_products_status", "status in ('draft','active','archived')");
        });
        b.HasKey(x => x.Id);
        b.Property(x => x.Id).HasDefaultValueSql("gen_random_uuid()");
        b.Property(x => x.Slug).HasColumnType("citext").IsRequired();
        b.HasIndex(x => x.Slug).IsUnique();
        b.Property(x => x.Sku).HasColumnType("text");
        b.HasIndex(x => x.Sku).IsUnique();
        b.Property(x => x.Name).HasColumnType("text").IsRequired();
        b.Property(x => x.ShortDescription).HasColumnType("text");
        b.Property(x => x.Description).HasColumnType("text");
        b.Property(x => x.BasePriceKobo).HasColumnType("bigint");
        b.Property(x => x.CompareAtPriceKobo).HasColumnType("bigint");
        b.Property(x => x.Material).HasColumnType("text");
        b.Property(x => x.DimensionsText).HasColumnType("text");
        b.Property(x => x.WidthCm).HasColumnType("numeric(8,2)");
        b.Property(x => x.DepthCm).HasColumnType("numeric(8,2)");
        b.Property(x => x.HeightCm).HasColumnType("numeric(8,2)");
        b.Property(x => x.IsMadeToOrder).HasDefaultValue(false);
        b.Property(x => x.IsBestseller).HasDefaultValue(false);
        b.Property(x => x.IsFeatured).HasDefaultValue(false);
        b.Property(x => x.IsPlaceholder).HasDefaultValue(false);
        b.Property(x => x.Badge).HasColumnType("text");
        b.Property(x => x.Status).HasColumnType("text").IsRequired().HasDefaultValue("draft");
        b.Property(x => x.CreatedAt).HasDefaultValueSql("now()");
        b.Property(x => x.UpdatedAt).HasDefaultValueSql("now()");
        b.HasIndex(x => x.CategoryId).HasDatabaseName("ix_products_category");
        b.HasIndex(x => x.Status).HasDatabaseName("ix_products_status");
        // gin trigram index for name search
        b.HasIndex(x => x.Name).HasMethod("gin").HasOperators("gin_trgm_ops")
            .HasDatabaseName("ix_products_name_trgm");
        b.HasOne(x => x.Category).WithMany(c => c.Products).HasForeignKey(x => x.CategoryId).OnDelete(DeleteBehavior.NoAction);

        // product_rooms join table (spec 4.2)
        b.HasMany(x => x.Rooms).WithMany(r => r.Products)
            .UsingEntity("product_rooms",
                l => l.HasOne(typeof(Room)).WithMany().HasForeignKey("room_id").OnDelete(DeleteBehavior.Cascade),
                r => r.HasOne(typeof(Product)).WithMany().HasForeignKey("product_id").OnDelete(DeleteBehavior.Cascade),
                j =>
                {
                    j.HasKey("product_id", "room_id");
                    j.IndexerProperty<Guid>("product_id");
                    j.IndexerProperty<Guid>("room_id");
                });
    }
}

public class ProductImageConfiguration : IEntityTypeConfiguration<ProductImage>
{
    public void Configure(EntityTypeBuilder<ProductImage> b)
    {
        b.ToTable("product_images", t =>
            t.HasCheckConstraint("ck_product_images_kind", "kind in ('studio','lifestyle','customer_home')"));
        b.HasKey(x => x.Id);
        b.Property(x => x.Id).HasDefaultValueSql("gen_random_uuid()");
        b.Property(x => x.Url).HasColumnType("text").IsRequired();
        b.Property(x => x.Alt).HasColumnType("text");
        b.Property(x => x.Kind).HasColumnType("text").IsRequired().HasDefaultValue("studio");
        b.Property(x => x.IsPrimary).HasDefaultValue(false);
        b.Property(x => x.SortOrder).HasDefaultValue(0);
        b.HasIndex(x => new { x.ProductId, x.SortOrder }).HasDatabaseName("ix_product_images_product");
        b.HasOne(x => x.Product).WithMany(p => p.Images).HasForeignKey(x => x.ProductId).OnDelete(DeleteBehavior.Cascade);
    }
}

public class ProductOptionGroupConfiguration : IEntityTypeConfiguration<ProductOptionGroup>
{
    public void Configure(EntityTypeBuilder<ProductOptionGroup> b)
    {
        b.ToTable("product_option_groups", t =>
            t.HasCheckConstraint("ck_option_groups_input_type", "input_type in ('select','swatch','radio')"));
        b.HasKey(x => x.Id);
        b.Property(x => x.Id).HasDefaultValueSql("gen_random_uuid()");
        b.Property(x => x.Name).HasColumnType("text").IsRequired();
        b.Property(x => x.StepOrder).HasDefaultValue(0);
        b.Property(x => x.InputType).HasColumnType("text").IsRequired().HasDefaultValue("select");
        b.Property(x => x.IsRequired).HasDefaultValue(true);
        b.HasIndex(x => new { x.ProductId, x.StepOrder }).HasDatabaseName("ix_option_groups_product");
        b.HasOne(x => x.Product).WithMany(p => p.OptionGroups).HasForeignKey(x => x.ProductId).OnDelete(DeleteBehavior.Cascade);
    }
}

public class ProductOptionValueConfiguration : IEntityTypeConfiguration<ProductOptionValue>
{
    public void Configure(EntityTypeBuilder<ProductOptionValue> b)
    {
        b.ToTable("product_option_values");
        b.HasKey(x => x.Id);
        b.Property(x => x.Id).HasDefaultValueSql("gen_random_uuid()");
        b.Property(x => x.Label).HasColumnType("text").IsRequired();
        b.Property(x => x.SwatchHex).HasColumnType("text");
        b.Property(x => x.ImageUrl).HasColumnType("text");
        b.Property(x => x.PriceDeltaKobo).HasColumnType("bigint").HasDefaultValue(0L);
        b.Property(x => x.IsDefault).HasDefaultValue(false);
        b.Property(x => x.SortOrder).HasDefaultValue(0);
        b.HasIndex(x => new { x.GroupId, x.SortOrder }).HasDatabaseName("ix_option_values_group");
        b.HasOne(x => x.Group).WithMany(g => g.Values).HasForeignKey(x => x.GroupId).OnDelete(DeleteBehavior.Cascade);
    }
}

public class DeliveryRateConfiguration : IEntityTypeConfiguration<DeliveryRate>
{
    public void Configure(EntityTypeBuilder<DeliveryRate> b)
    {
        b.ToTable("delivery_rates", t =>
            t.HasCheckConstraint("ck_delivery_rates_fee", "fee_kobo >= 0"));
        b.HasKey(x => x.Id);
        b.Property(x => x.Id).HasDefaultValueSql("gen_random_uuid()");
        b.Property(x => x.State).HasColumnType("text").IsRequired();
        b.HasIndex(x => x.State).IsUnique();
        b.Property(x => x.FeeKobo).HasColumnType("bigint");
        b.Property(x => x.IsActive).HasDefaultValue(true);
    }
}

public class ShowroomConfiguration : IEntityTypeConfiguration<Showroom>
{
    public void Configure(EntityTypeBuilder<Showroom> b)
    {
        b.ToTable("showrooms");
        b.HasKey(x => x.Id);
        b.Property(x => x.Id).HasDefaultValueSql("gen_random_uuid()");
        b.Property(x => x.Slug).HasColumnType("citext").IsRequired();
        b.HasIndex(x => x.Slug).IsUnique();
        b.Property(x => x.Name).HasColumnType("text").IsRequired();
        b.Property(x => x.City).HasColumnType("text").IsRequired();
        b.Property(x => x.State).HasColumnType("text").IsRequired();
        b.Property(x => x.Address).HasColumnType("text").IsRequired();
        b.Property(x => x.Phone).HasColumnType("text");
        b.Property(x => x.Whatsapp).HasColumnType("text");
        b.Property(x => x.Latitude).HasColumnType("numeric(9,6)");
        b.Property(x => x.Longitude).HasColumnType("numeric(9,6)");
        b.Property(x => x.OpeningHoursJson).HasColumnType("jsonb").IsRequired().HasDefaultValue("{}");
        b.Property(x => x.IsActive).HasDefaultValue(true);
    }
}
