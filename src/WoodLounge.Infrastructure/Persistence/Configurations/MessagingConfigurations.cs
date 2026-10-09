using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WoodLounge.Domain.Messaging;

namespace WoodLounge.Infrastructure.Persistence.Configurations;

public class MessageThreadConfiguration : IEntityTypeConfiguration<MessageThread>
{
    public void Configure(EntityTypeBuilder<MessageThread> b)
    {
        b.ToTable("message_threads", t =>
        {
            t.HasCheckConstraint("ck_threads_kind", "kind in ('custom_request','site_visit')");
            t.HasCheckConstraint("ck_threads_kind_target",
                "(kind = 'custom_request' and custom_request_id is not null and site_visit_id is null) or " +
                "(kind = 'site_visit' and site_visit_id is not null and custom_request_id is null)");
        });
        b.HasKey(x => x.Id);
        b.Property(x => x.Id).HasDefaultValueSql("gen_random_uuid()");
        b.Property(x => x.Kind).HasColumnType("text").IsRequired();
        b.Property(x => x.LastMessageAt).HasDefaultValueSql("now()");
        b.Property(x => x.CustomerUnreadCount).HasDefaultValue(0);
        b.Property(x => x.AdminUnreadCount).HasDefaultValue(0);
        b.Property(x => x.CreatedAt).HasDefaultValueSql("now()");
        // partial unique indexes
        b.HasIndex(x => x.CustomRequestId).IsUnique()
            .HasFilter("custom_request_id is not null").HasDatabaseName("ux_thread_custom");
        b.HasIndex(x => x.SiteVisitId).IsUnique()
            .HasFilter("site_visit_id is not null").HasDatabaseName("ux_thread_visit");
        b.HasOne(x => x.CustomRequest).WithMany().HasForeignKey(x => x.CustomRequestId).OnDelete(DeleteBehavior.Cascade);
        b.HasOne(x => x.SiteVisit).WithMany().HasForeignKey(x => x.SiteVisitId).OnDelete(DeleteBehavior.Cascade);
        b.HasOne(x => x.User).WithMany().HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.NoAction);
    }
}

public class MessageConfiguration : IEntityTypeConfiguration<Message>
{
    public void Configure(EntityTypeBuilder<Message> b)
    {
        b.ToTable("messages", t =>
        {
            t.HasCheckConstraint("ck_messages_sender_role", "sender_role in ('customer','admin','system')");
            t.HasCheckConstraint("ck_messages_kind", "kind in ('text','quote','status_change','system')");
        });
        b.HasKey(x => x.Id);
        b.Property(x => x.Id).HasDefaultValueSql("gen_random_uuid()");
        b.Property(x => x.SenderRole).HasColumnType("text").IsRequired();
        b.Property(x => x.Kind).HasColumnType("text").IsRequired().HasDefaultValue("text");
        b.Property(x => x.Body).HasColumnType("text").IsRequired();
        b.Property(x => x.CreatedAt).HasDefaultValueSql("now()");
        b.HasIndex(x => new { x.ThreadId, x.CreatedAt }).HasDatabaseName("ix_messages_thread");
        b.HasOne(x => x.Thread).WithMany(t => t.Messages).HasForeignKey(x => x.ThreadId).OnDelete(DeleteBehavior.Cascade);
        b.HasOne(x => x.SenderUser).WithMany().HasForeignKey(x => x.SenderUserId).OnDelete(DeleteBehavior.NoAction);
        b.HasOne(x => x.Quote).WithMany().HasForeignKey(x => x.QuoteId).OnDelete(DeleteBehavior.NoAction);
    }
}

public class MessageAttachmentConfiguration : IEntityTypeConfiguration<MessageAttachment>
{
    public void Configure(EntityTypeBuilder<MessageAttachment> b)
    {
        b.ToTable("message_attachments");
        b.HasKey(x => x.Id);
        b.Property(x => x.Id).HasDefaultValueSql("gen_random_uuid()");
        b.Property(x => x.FileKey).HasColumnType("text").IsRequired();
        b.Property(x => x.FileName).HasColumnType("text").IsRequired();
        b.Property(x => x.ContentType).HasColumnType("text").IsRequired();
        b.Property(x => x.SizeBytes).HasColumnType("bigint");
        b.HasOne(x => x.Message).WithMany(m => m.Attachments).HasForeignKey(x => x.MessageId).OnDelete(DeleteBehavior.Cascade);
    }
}
