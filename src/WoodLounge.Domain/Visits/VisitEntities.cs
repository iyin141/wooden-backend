using WoodLounge.Domain.Catalog;
using WoodLounge.Domain.Common;
using WoodLounge.Domain.Identity;

namespace WoodLounge.Domain.Visits;

public class VisitAvailability : Entity
{
    public string VisitType { get; set; } = null!;  // site_inspection | online_visit | showroom_visit
    public Guid? ShowroomId { get; set; }
    public Showroom? Showroom { get; set; }
    public short Weekday { get; set; }              // 0 = Sunday, 0..6
    public TimeOnly StartTime { get; set; }
    public TimeOnly EndTime { get; set; }
    public int SlotMinutes { get; set; } = 60;
    public int Capacity { get; set; } = 1;
    public bool IsActive { get; set; } = true;
}

public class VisitBlackout : Entity
{
    public DateOnly OnDate { get; set; }
    public string? VisitType { get; set; }          // nullable = all types
    public Guid? ShowroomId { get; set; }
    public Showroom? Showroom { get; set; }
    public string? Reason { get; set; }
}

public class SiteVisitBooking : AuditedEntity
{
    public string Reference { get; set; } = null!;  // e.g. VB-20481 (from sequence)
    public Guid UserId { get; set; }
    public User? User { get; set; }
    public string VisitType { get; set; } = null!;
    public Guid? ShowroomId { get; set; }
    public Showroom? Showroom { get; set; }
    public DateOnly RequestedDate { get; set; }
    public TimeOnly RequestedTime { get; set; }
    public string ContactName { get; set; } = null!;
    public string ContactPhone { get; set; } = null!;
    public string? ContactEmail { get; set; }       // citext
    public string? SiteAddress { get; set; }        // required for site_inspection
    public string? SiteCity { get; set; }
    public string? SiteState { get; set; }
    public string? ProjectNotes { get; set; }
    public string Status { get; set; } = "pending"; // pending | confirmed | reschedule_proposed | declined | completed | cancelled
    public DateOnly? ProposedDate { get; set; }
    public TimeOnly? ProposedTime { get; set; }
    public DateOnly? ConfirmedDate { get; set; }
    public TimeOnly? ConfirmedTime { get; set; }
    public string? MeetingLink { get; set; }        // online visits
    public string? DeclineReason { get; set; }
    public Guid? HandledById { get; set; }
    public User? HandledBy { get; set; }
}
