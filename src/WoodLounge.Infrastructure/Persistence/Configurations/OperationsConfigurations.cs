using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WoodLounge.Domain.Operations;

namespace WoodLounge.Infrastructure.Persistence.Configurations;

public class NotificationConfiguration : IEntityTypeConfiguration<Notification>
{
    public void Configure(EntityTypeBuilder<Notification> b)
    {
        b.ToTable("notifications", t =>
        {
            t.HasCheckConstraint("ck_notifications_channel", "channel in ('email','whatsapp','sms')");
            t.HasCheckConstraint("ck_notifications_status", "status in ('queued','sent','failed')");
        });
        b.HasKey(x => x.Id);
        b.Property(x => x.Id).HasDefaultValueSql("gen_random_uuid()");
        b.Property(x => x.Channel).HasColumnType("text").IsRequired();
        b.Property(x => x.ToAddress).HasColumnType("text").IsRequired();
        b.Property(x => x.Template).HasColumnType("text").IsRequired();
        b.Property(x => x.PayloadJson).HasColumnName("payload").HasColumnType("jsonb").IsRequired().HasDefaultValue("{}");
        b.Property(x => x.Status).HasColumnType("text").IsRequired().HasDefaultValue("queued");
        b.Property(x => x.Attempts).HasDefaultValue(0);
        b.Property(x => x.LastError).HasColumnType("text");
        b.Property(x => x.ScheduledAt).HasDefaultValueSql("now()");
        b.Property(x => x.CreatedAt).HasDefaultValueSql("now()");
        b.HasIndex(x => new { x.Status, x.ScheduledAt }).HasDatabaseName("ix_notifications_queue");
        b.HasOne(x => x.User).WithMany().HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.NoAction);
    }
}

public class AuditLogConfiguration : IEntityTypeConfiguration<AuditLog>
{
    public void Configure(EntityTypeBuilder<AuditLog> b)
    {
        b.ToTable("audit_log");
        b.HasKey(x => x.Id);
        b.Property(x => x.Id).UseIdentityAlwaysColumn(); // bigint generated always as identity
        b.Property(x => x.Action).HasColumnType("text").IsRequired();
        b.Property(x => x.EntityType).HasColumnType("text").IsRequired();
        b.Property(x => x.EntityId).HasColumnType("text").IsRequired();
        b.Property(x => x.BeforeJson).HasColumnName("before").HasColumnType("jsonb");
        b.Property(x => x.AfterJson).HasColumnName("after").HasColumnType("jsonb");
        b.Property(x => x.Ip).HasColumnType("inet");
        b.Property(x => x.CreatedAt).HasDefaultValueSql("now()");
        b.HasIndex(x => new { x.EntityType, x.EntityId, x.CreatedAt })
            .IsDescending(false, false, true).HasDatabaseName("ix_audit_entity");
        b.HasOne(x => x.ActorUser).WithMany().HasForeignKey(x => x.ActorUserId).OnDelete(DeleteBehavior.NoAction);
    }
}

public class SiteSettingConfiguration : IEntityTypeConfiguration<SiteSetting>
{
    public void Configure(EntityTypeBuilder<SiteSetting> b)
    {
        b.ToTable("site_settings");
        b.HasKey(x => x.Key);
        b.Property(x => x.Key).HasColumnType("text");
        b.Property(x => x.ValueJson).HasColumnName("value").HasColumnType("jsonb").IsRequired();
        b.Property(x => x.UpdatedAt).HasDefaultValueSql("now()");
        b.HasOne(x => x.UpdatedBy).WithMany().HasForeignKey(x => x.UpdatedById).OnDelete(DeleteBehavior.NoAction);
    }
}
