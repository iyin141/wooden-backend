using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WoodLounge.Domain.Orders;

namespace WoodLounge.Infrastructure.Persistence.Configurations;

public class OrderConfiguration : IEntityTypeConfiguration<Order>
{
    public void Configure(EntityTypeBuilder<Order> b)
    {
        b.ToTable("orders", t =>
        {
            t.HasCheckConstraint("ck_orders_status", "status in (" +
                "'pending_payment','paid','processing','ready','out_for_delivery','delivered'," +
                "'quote_accepted','deposit_paid','in_production','cancelled','refunded')");
            t.HasCheckConstraint("ck_orders_kind", "kind in ('premade','custom')");
            t.HasCheckConstraint("ck_orders_subtotal", "subtotal_kobo >= 0");
            t.HasCheckConstraint("ck_orders_delivery_fee", "delivery_fee_kobo >= 0");
            t.HasCheckConstraint("ck_orders_discount", "discount_kobo >= 0");
            t.HasCheckConstraint("ck_orders_total", "total_kobo >= 0");
            t.HasCheckConstraint("ck_orders_amount_paid", "amount_paid_kobo >= 0");
            t.HasCheckConstraint("ck_orders_contact", "user_id is not null or guest_email is not null or guest_phone is not null");
            t.HasCheckConstraint("ck_orders_custom_refs", "kind <> 'custom' or (custom_request_id is not null and quote_id is not null)");
        });
        b.HasKey(x => x.Id);
        b.Property(x => x.Id).HasDefaultValueSql("gen_random_uuid()");
        // order_number default from sequence (sequence declared in WoodLoungeDbContext model below)
        b.Property(x => x.OrderNumber).HasColumnType("text").IsRequired()
            .HasDefaultValueSql("('WL-' || nextval('order_number_seq'))");
        b.HasIndex(x => x.OrderNumber).IsUnique();
        b.Property(x => x.GuestEmail).HasColumnType("citext");
        b.Property(x => x.GuestPhone).HasColumnType("text");
        b.Property(x => x.CustomerName).HasColumnType("text").IsRequired();
        b.Property(x => x.Kind).HasColumnType("text").IsRequired();
        b.Property(x => x.Status).HasColumnType("text").IsRequired().HasDefaultValue("pending_payment");
        b.Property(x => x.SubtotalKobo).HasColumnType("bigint");
        b.Property(x => x.DeliveryFeeKobo).HasColumnType("bigint").HasDefaultValue(0L);
        b.Property(x => x.DiscountKobo).HasColumnType("bigint").HasDefaultValue(0L);
        b.Property(x => x.TotalKobo).HasColumnType("bigint");
        b.Property(x => x.AmountPaidKobo).HasColumnType("bigint").HasDefaultValue(0L);
        // generated column: balance_due_kobo = total - amount_paid (stored, read-only from EF)
        b.Property(x => x.BalanceDueKobo)
            .HasColumnName("balance_due_kobo")
            .HasColumnType("bigint")
            .HasComputedColumnSql("total_kobo - amount_paid_kobo", stored: true)
            .ValueGeneratedOnAddOrUpdate();
        b.Property(x => x.ShipName).HasColumnType("text").IsRequired();
        b.Property(x => x.ShipPhone).HasColumnType("text").IsRequired();
        b.Property(x => x.ShipLine1).HasColumnType("text").IsRequired();
        b.Property(x => x.ShipLine2).HasColumnType("text");
        b.Property(x => x.ShipCity).HasColumnType("text").IsRequired();
        b.Property(x => x.ShipState).HasColumnType("text").IsRequired();
        b.Property(x => x.ShipNotes).HasColumnType("text");
        b.Property(x => x.AdminNote).HasColumnType("text");
        b.Property(x => x.CreatedAt).HasDefaultValueSql("now()");
        b.Property(x => x.UpdatedAt).HasDefaultValueSql("now()");
        b.HasIndex(x => x.UserId).HasDatabaseName("ix_orders_user");
        b.HasIndex(x => new { x.Status, x.CreatedAt }).IsDescending(false, true)
            .HasDatabaseName("ix_orders_status_created");
        b.HasIndex(x => x.GuestEmail).HasDatabaseName("ix_orders_guest_email");
        b.HasOne(x => x.User).WithMany().HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.NoAction);
        b.HasOne(x => x.CustomRequest).WithMany().HasForeignKey(x => x.CustomRequestId).OnDelete(DeleteBehavior.NoAction);
        b.HasOne(x => x.Quote).WithMany().HasForeignKey(x => x.QuoteId).OnDelete(DeleteBehavior.NoAction);
    }
}

public class OrderItemConfiguration : IEntityTypeConfiguration<OrderItem>
{
    public void Configure(EntityTypeBuilder<OrderItem> b)
    {
        b.ToTable("order_items", t =>
        {
            t.HasCheckConstraint("ck_order_items_unit_price", "unit_price_kobo >= 0");
            t.HasCheckConstraint("ck_order_items_qty", "quantity > 0");
            t.HasCheckConstraint("ck_order_items_line_total", "line_total_kobo >= 0");
        });
        b.HasKey(x => x.Id);
        b.Property(x => x.Id).HasDefaultValueSql("gen_random_uuid()");
        b.Property(x => x.NameSnapshot).HasColumnType("text").IsRequired();
        b.Property(x => x.SkuSnapshot).HasColumnType("text");
        b.Property(x => x.ImageUrlSnapshot).HasColumnType("text");
        b.Property(x => x.UnitPriceKobo).HasColumnType("bigint");
        b.Property(x => x.SelectedOptionsJson).HasColumnName("selected_options").HasColumnType("jsonb")
            .IsRequired().HasDefaultValue("[]");
        b.Property(x => x.LineTotalKobo).HasColumnType("bigint");
        b.HasIndex(x => x.OrderId).HasDatabaseName("ix_order_items_order");
        b.HasOne(x => x.Order).WithMany(o => o.Items).HasForeignKey(x => x.OrderId).OnDelete(DeleteBehavior.Cascade);
        b.HasOne(x => x.Product).WithMany().HasForeignKey(x => x.ProductId).OnDelete(DeleteBehavior.SetNull);
    }
}

public class OrderStatusHistoryConfiguration : IEntityTypeConfiguration<OrderStatusHistory>
{
    public void Configure(EntityTypeBuilder<OrderStatusHistory> b)
    {
        b.ToTable("order_status_history");
        b.HasKey(x => x.Id);
        b.Property(x => x.Id).HasDefaultValueSql("gen_random_uuid()");
        b.Property(x => x.Status).HasColumnType("text").IsRequired();
        b.Property(x => x.Note).HasColumnType("text");
        b.Property(x => x.NotifyCustomer).HasDefaultValue(true);
        b.Property(x => x.CreatedAt).HasDefaultValueSql("now()");
        b.HasIndex(x => new { x.OrderId, x.CreatedAt }).HasDatabaseName("ix_order_history_order");
        b.HasOne(x => x.Order).WithMany(o => o.StatusHistory).HasForeignKey(x => x.OrderId).OnDelete(DeleteBehavior.Cascade);
        b.HasOne(x => x.ChangedBy).WithMany().HasForeignKey(x => x.ChangedById).OnDelete(DeleteBehavior.NoAction);
    }
}
