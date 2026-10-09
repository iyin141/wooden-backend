using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WoodLounge.Domain.Payments;

namespace WoodLounge.Infrastructure.Persistence.Configurations;

public class PaymentConfiguration : IEntityTypeConfiguration<Payment>
{
    public void Configure(EntityTypeBuilder<Payment> b)
    {
        b.ToTable("payments", t =>
        {
            t.HasCheckConstraint("ck_payments_kind", "kind in ('full','deposit','balance')");
            t.HasCheckConstraint("ck_payments_amount", "amount_kobo > 0");
            t.HasCheckConstraint("ck_payments_status", "status in ('initialized','pending','success','failed','abandoned','refunded')");
        });
        b.HasKey(x => x.Id);
        b.Property(x => x.Id).HasDefaultValueSql("gen_random_uuid()");
        b.Property(x => x.Kind).HasColumnType("text").IsRequired();
        b.Property(x => x.Provider).HasColumnType("text").IsRequired().HasDefaultValue("paystack");
        b.Property(x => x.Reference).HasColumnType("text").IsRequired();
        b.HasIndex(x => x.Reference).IsUnique();
        b.Property(x => x.AmountKobo).HasColumnType("bigint");
        b.Property(x => x.Currency).HasColumnType("text").IsRequired().HasDefaultValue("NGN");
        b.Property(x => x.Status).HasColumnType("text").IsRequired().HasDefaultValue("initialized");
        b.Property(x => x.Channel).HasColumnType("text");
        b.Property(x => x.FeesKobo).HasColumnType("bigint");
        b.Property(x => x.CustomerEmail).HasColumnType("citext").IsRequired();
        b.Property(x => x.AuthorizationUrl).HasColumnType("text");
        b.Property(x => x.AccessCode).HasColumnType("text");
        b.Property(x => x.FailureReason).HasColumnType("text");
        b.Property(x => x.RawJson).HasColumnName("raw").HasColumnType("jsonb");
        b.Property(x => x.CreatedAt).HasDefaultValueSql("now()");
        b.Property(x => x.UpdatedAt).HasDefaultValueSql("now()");
        b.HasIndex(x => x.OrderId).HasDatabaseName("ix_payments_order");
        b.HasIndex(x => new { x.Status, x.CreatedAt }).IsDescending(false, true)
            .HasDatabaseName("ix_payments_status_created");
        // spec: order_id references orders(id) without cascade
        b.HasOne(x => x.Order).WithMany(o => o.Payments).HasForeignKey(x => x.OrderId).OnDelete(DeleteBehavior.NoAction);
    }
}

public class PaymentWebhookEventConfiguration : IEntityTypeConfiguration<PaymentWebhookEvent>
{
    public void Configure(EntityTypeBuilder<PaymentWebhookEvent> b)
    {
        b.ToTable("payment_webhook_events");
        b.HasKey(x => x.Id);
        b.Property(x => x.Id).HasDefaultValueSql("gen_random_uuid()");
        b.Property(x => x.Provider).HasColumnType("text").IsRequired().HasDefaultValue("paystack");
        b.Property(x => x.EventId).HasColumnType("text").IsRequired();
        b.Property(x => x.EventType).HasColumnType("text").IsRequired();
        b.Property(x => x.PayloadJson).HasColumnName("payload").HasColumnType("jsonb").IsRequired();
        b.Property(x => x.Error).HasColumnType("text");
        b.Property(x => x.ReceivedAt).HasDefaultValueSql("now()");
        b.HasIndex(x => new { x.Provider, x.EventId }).IsUnique(); // idempotency
    }
}
