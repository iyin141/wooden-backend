using Microsoft.EntityFrameworkCore;
using WoodLounge.Domain.Catalog;
using WoodLounge.Domain.Common;
using WoodLounge.Domain.CustomOrders;
using WoodLounge.Domain.Identity;
using WoodLounge.Domain.Messaging;
using WoodLounge.Domain.Operations;
using WoodLounge.Domain.Orders;
using WoodLounge.Domain.Payments;
using WoodLounge.Domain.Visits;

namespace WoodLounge.Infrastructure.Persistence;

public class WoodLoungeDbContext(DbContextOptions<WoodLoungeDbContext> options) : DbContext(options)
{
    // Identity
    public DbSet<User> Users => Set<User>();
    public DbSet<OtpCode> OtpCodes => Set<OtpCode>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
    public DbSet<Address> Addresses => Set<Address>();

    // Catalog
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Room> Rooms => Set<Room>();
    public DbSet<Product> Products => Set<Product>();
    public DbSet<ProductImage> ProductImages => Set<ProductImage>();
    public DbSet<ProductOptionGroup> ProductOptionGroups => Set<ProductOptionGroup>();
    public DbSet<ProductOptionValue> ProductOptionValues => Set<ProductOptionValue>();
    public DbSet<DeliveryRate> DeliveryRates => Set<DeliveryRate>();
    public DbSet<Showroom> Showrooms => Set<Showroom>();

    // Visits
    public DbSet<VisitAvailability> VisitAvailabilities => Set<VisitAvailability>();
    public DbSet<VisitBlackout> VisitBlackouts => Set<VisitBlackout>();
    public DbSet<SiteVisitBooking> SiteVisitBookings => Set<SiteVisitBooking>();

    // Custom orders + quotes
    public DbSet<CustomRequest> CustomRequests => Set<CustomRequest>();
    public DbSet<CustomRequestAttachment> CustomRequestAttachments => Set<CustomRequestAttachment>();
    public DbSet<Quote> Quotes => Set<Quote>();

    // Messaging
    public DbSet<MessageThread> MessageThreads => Set<MessageThread>();
    public DbSet<Message> Messages => Set<Message>();
    public DbSet<MessageAttachment> MessageAttachments => Set<MessageAttachment>();

    // Orders
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OrderItem> OrderItems => Set<OrderItem>();
    public DbSet<OrderStatusHistory> OrderStatusHistories => Set<OrderStatusHistory>();

    // Payments
    public DbSet<Payment> Payments => Set<Payment>();
    public DbSet<PaymentWebhookEvent> PaymentWebhookEvents => Set<PaymentWebhookEvent>();

    // Operations
    public DbSet<Notification> Notifications => Set<Notification>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();
    public DbSet<SiteSetting> SiteSettings => Set<SiteSetting>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Extensions (spec §4). Created via migration SQL; declared here for model awareness.
        modelBuilder.HasPostgresExtension("pgcrypto");
        modelBuilder.HasPostgresExtension("citext");
        modelBuilder.HasPostgresExtension("pg_trgm");

        // Reference sequences (spec §4.6)
        modelBuilder.HasSequence<long>("order_number_seq").StartsAt(10001);
        modelBuilder.HasSequence<long>("visit_ref_seq").StartsAt(20001);
        modelBuilder.HasSequence<long>("request_ref_seq").StartsAt(30001);

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(WoodLoungeDbContext).Assembly);
    }

    /// <summary>Sets updated_at on every modified AuditedEntity (spec §4: EF, not a trigger).</summary>
    public override int SaveChanges(bool acceptAllChangesOnSuccess)
    {
        StampUpdatedAt();
        return base.SaveChanges(acceptAllChangesOnSuccess);
    }

    public override Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default)
    {
        StampUpdatedAt();
        return base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
    }

    private void StampUpdatedAt()
    {
        var now = DateTimeOffset.UtcNow;
        foreach (var entry in ChangeTracker.Entries<AuditedEntity>())
        {
            if (entry.State == EntityState.Added)
            {
                if (entry.Entity.CreatedAt == default) entry.Entity.CreatedAt = now;
                entry.Entity.UpdatedAt = now;
            }
            else if (entry.State == EntityState.Modified)
            {
                entry.Entity.UpdatedAt = now;
            }
        }
    }
}
