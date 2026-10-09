using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WoodLounge.Domain.CustomOrders;
using WoodLounge.Domain.Visits;

namespace WoodLounge.Infrastructure.Persistence.Configurations;

public class VisitAvailabilityConfiguration : IEntityTypeConfiguration<VisitAvailability>
{
    public void Configure(EntityTypeBuilder<VisitAvailability> b)
    {
        b.ToTable("visit_availability", t =>
        {
            t.HasCheckConstraint("ck_visit_availability_type", "visit_type in ('site_inspection','online_visit','showroom_visit')");
            t.HasCheckConstraint("ck_visit_availability_weekday", "weekday between 0 and 6");
            t.HasCheckConstraint("ck_visit_availability_time", "end_time > start_time");
        });
        b.HasKey(x => x.Id);
        b.Property(x => x.Id).HasDefaultValueSql("gen_random_uuid()");
        b.Property(x => x.VisitType).HasColumnType("text").IsRequired();
        b.Property(x => x.SlotMinutes).HasDefaultValue(60);
        b.Property(x => x.Capacity).HasDefaultValue(1);
        b.Property(x => x.IsActive).HasDefaultValue(true);
        b.HasOne(x => x.Showroom).WithMany().HasForeignKey(x => x.ShowroomId).OnDelete(DeleteBehavior.NoAction);
    }
}

public class VisitBlackoutConfiguration : IEntityTypeConfiguration<VisitBlackout>
{
    public void Configure(EntityTypeBuilder<VisitBlackout> b)
    {
        b.ToTable("visit_blackouts", t =>
            t.HasCheckConstraint("ck_visit_blackouts_type", "visit_type in ('site_inspection','online_visit','showroom_visit')"));
        b.HasKey(x => x.Id);
        b.Property(x => x.Id).HasDefaultValueSql("gen_random_uuid()");
        b.Property(x => x.VisitType).HasColumnType("text");
        b.Property(x => x.Reason).HasColumnType("text");
        b.HasOne(x => x.Showroom).WithMany().HasForeignKey(x => x.ShowroomId).OnDelete(DeleteBehavior.NoAction);
    }
}

public class SiteVisitBookingConfiguration : IEntityTypeConfiguration<SiteVisitBooking>
{
    public void Configure(EntityTypeBuilder<SiteVisitBooking> b)
    {
        b.ToTable("site_visit_bookings", t =>
        {
            t.HasCheckConstraint("ck_visits_type", "visit_type in ('site_inspection','online_visit','showroom_visit')");
            t.HasCheckConstraint("ck_visits_status", "status in ('pending','confirmed','reschedule_proposed','declined','completed','cancelled')");
            t.HasCheckConstraint("ck_visits_site_address", "visit_type <> 'site_inspection' or site_address is not null");
            t.HasCheckConstraint("ck_visits_showroom", "visit_type <> 'showroom_visit' or showroom_id is not null");
        });
        b.HasKey(x => x.Id);
        b.Property(x => x.Id).HasDefaultValueSql("gen_random_uuid()");
        b.Property(x => x.Reference).HasColumnType("text").IsRequired();
        b.HasIndex(x => x.Reference).IsUnique();
        b.Property(x => x.VisitType).HasColumnType("text").IsRequired();
        b.Property(x => x.ContactName).HasColumnType("text").IsRequired();
        b.Property(x => x.ContactPhone).HasColumnType("text").IsRequired();
        b.Property(x => x.ContactEmail).HasColumnType("citext");
        b.Property(x => x.SiteAddress).HasColumnType("text");
        b.Property(x => x.SiteCity).HasColumnType("text");
        b.Property(x => x.SiteState).HasColumnType("text");
        b.Property(x => x.ProjectNotes).HasColumnType("text");
        b.Property(x => x.Status).HasColumnType("text").IsRequired().HasDefaultValue("pending");
        b.Property(x => x.MeetingLink).HasColumnType("text");
        b.Property(x => x.DeclineReason).HasColumnType("text");
        b.Property(x => x.CreatedAt).HasDefaultValueSql("now()");
        b.Property(x => x.UpdatedAt).HasDefaultValueSql("now()");
        b.HasIndex(x => x.UserId).HasDatabaseName("ix_visits_user");
        b.HasIndex(x => new { x.Status, x.RequestedDate }).HasDatabaseName("ix_visits_status_date");
        b.HasOne(x => x.User).WithMany().HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.NoAction);
        b.HasOne(x => x.Showroom).WithMany().HasForeignKey(x => x.ShowroomId).OnDelete(DeleteBehavior.NoAction);
        b.HasOne(x => x.HandledBy).WithMany().HasForeignKey(x => x.HandledById).OnDelete(DeleteBehavior.NoAction);
    }
}

public class CustomRequestConfiguration : IEntityTypeConfiguration<CustomRequest>
{
    public void Configure(EntityTypeBuilder<CustomRequest> b)
    {
        b.ToTable("custom_requests", t =>
        {
            t.HasCheckConstraint("ck_custom_requests_tier", "tier in ('modify_existing','fully_bespoke','full_room')");
            t.HasCheckConstraint("ck_custom_requests_budget_min", "budget_min_kobo is null or budget_min_kobo >= 0");
            t.HasCheckConstraint("ck_custom_requests_budget_max", "budget_max_kobo is null or budget_max_kobo >= 0");
            t.HasCheckConstraint("ck_custom_requests_status", "status in ('submitted','in_discussion','quoted','quote_accepted','quote_declined','closed','cancelled')");
        });
        b.HasKey(x => x.Id);
        b.Property(x => x.Id).HasDefaultValueSql("gen_random_uuid()");
        b.Property(x => x.Reference).HasColumnType("text").IsRequired();
        b.HasIndex(x => x.Reference).IsUnique();
        b.Property(x => x.Tier).HasColumnType("text").IsRequired();
        b.Property(x => x.Title).HasColumnType("text").IsRequired();
        b.Property(x => x.Description).HasColumnType("text").IsRequired();
        b.Property(x => x.DimensionsText).HasColumnType("text");
        b.Property(x => x.PreferredMaterials).HasColumnType("text");
        b.Property(x => x.BudgetMinKobo).HasColumnType("bigint");
        b.Property(x => x.BudgetMaxKobo).HasColumnType("bigint");
        b.Property(x => x.DeliveryCity).HasColumnType("text");
        b.Property(x => x.DeliveryState).HasColumnType("text");
        b.Property(x => x.ContactName).HasColumnType("text").IsRequired();
        b.Property(x => x.ContactPhone).HasColumnType("text").IsRequired();
        b.Property(x => x.ContactEmail).HasColumnType("citext");
        b.Property(x => x.Status).HasColumnType("text").IsRequired().HasDefaultValue("submitted");
        b.Property(x => x.CreatedAt).HasDefaultValueSql("now()");
        b.Property(x => x.UpdatedAt).HasDefaultValueSql("now()");
        b.HasIndex(x => x.UserId).HasDatabaseName("ix_custom_requests_user");
        b.HasIndex(x => new { x.Status, x.CreatedAt }).IsDescending(false, true)
            .HasDatabaseName("ix_custom_requests_status");
        b.HasOne(x => x.User).WithMany().HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.NoAction);
        b.HasOne(x => x.BaseProduct).WithMany().HasForeignKey(x => x.BaseProductId).OnDelete(DeleteBehavior.NoAction);
        b.HasOne(x => x.Room).WithMany().HasForeignKey(x => x.RoomId).OnDelete(DeleteBehavior.NoAction);
        b.HasOne(x => x.AssignedAdmin).WithMany().HasForeignKey(x => x.AssignedAdminId).OnDelete(DeleteBehavior.NoAction);
    }
}

public class CustomRequestAttachmentConfiguration : IEntityTypeConfiguration<CustomRequestAttachment>
{
    public void Configure(EntityTypeBuilder<CustomRequestAttachment> b)
    {
        b.ToTable("custom_request_attachments");
        b.HasKey(x => x.Id);
        b.Property(x => x.Id).HasDefaultValueSql("gen_random_uuid()");
        b.Property(x => x.FileKey).HasColumnType("text").IsRequired();
        b.Property(x => x.FileName).HasColumnType("text").IsRequired();
        b.Property(x => x.ContentType).HasColumnType("text").IsRequired();
        b.Property(x => x.SizeBytes).HasColumnType("bigint");
        b.Property(x => x.CreatedAt).HasDefaultValueSql("now()");
        b.HasOne(x => x.Request).WithMany(r => r.Attachments).HasForeignKey(x => x.RequestId).OnDelete(DeleteBehavior.Cascade);
    }
}

public class QuoteConfiguration : IEntityTypeConfiguration<Quote>
{
    public void Configure(EntityTypeBuilder<Quote> b)
    {
        b.ToTable("quotes", t =>
        {
            t.HasCheckConstraint("ck_quotes_amount", "amount_kobo > 0");
            t.HasCheckConstraint("ck_quotes_deposit", "deposit_kobo > 0 and deposit_kobo <= amount_kobo");
            t.HasCheckConstraint("ck_quotes_timeline", "timeline_weeks > 0");
            t.HasCheckConstraint("ck_quotes_status", "status in ('sent','accepted','declined','expired','superseded')");
        });
        b.HasKey(x => x.Id);
        b.Property(x => x.Id).HasDefaultValueSql("gen_random_uuid()");
        b.Property(x => x.Version).HasDefaultValue(1);
        b.Property(x => x.AmountKobo).HasColumnType("bigint");
        b.Property(x => x.DepositKobo).HasColumnType("bigint");
        b.Property(x => x.Notes).HasColumnType("text");
        b.Property(x => x.Status).HasColumnType("text").IsRequired().HasDefaultValue("sent");
        b.Property(x => x.SentAt).HasDefaultValueSql("now()");
        b.HasIndex(x => new { x.CustomRequestId, x.Version }).IsUnique();
        b.HasOne(x => x.CustomRequest).WithMany(r => r.Quotes).HasForeignKey(x => x.CustomRequestId).OnDelete(DeleteBehavior.Cascade);
        b.HasOne(x => x.SentBy).WithMany().HasForeignKey(x => x.SentById).OnDelete(DeleteBehavior.NoAction);
    }
}
