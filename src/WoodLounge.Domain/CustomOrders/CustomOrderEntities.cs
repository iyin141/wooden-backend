using WoodLounge.Domain.Catalog;
using WoodLounge.Domain.Common;
using WoodLounge.Domain.Identity;

namespace WoodLounge.Domain.CustomOrders;

public class CustomRequest : AuditedEntity
{
    public string Reference { get; set; } = null!;  // e.g. CR-30112
    public Guid UserId { get; set; }
    public User? User { get; set; }
    public string Tier { get; set; } = null!;       // modify_existing | fully_bespoke | full_room
    public Guid? BaseProductId { get; set; }
    public Product? BaseProduct { get; set; }
    public Guid? RoomId { get; set; }
    public Room? Room { get; set; }
    public string Title { get; set; } = null!;
    public string Description { get; set; } = null!;
    public string? DimensionsText { get; set; }
    public string? PreferredMaterials { get; set; }
    public long? BudgetMinKobo { get; set; }
    public long? BudgetMaxKobo { get; set; }
    public DateOnly? TargetDate { get; set; }
    public string? DeliveryCity { get; set; }
    public string? DeliveryState { get; set; }
    public string ContactName { get; set; } = null!;
    public string ContactPhone { get; set; } = null!;
    public string? ContactEmail { get; set; }       // citext
    public string Status { get; set; } = "submitted"; // submitted | in_discussion | quoted | quote_accepted | quote_declined | closed | cancelled
    public Guid? AssignedAdminId { get; set; }
    public User? AssignedAdmin { get; set; }

    public ICollection<CustomRequestAttachment> Attachments { get; set; } = new List<CustomRequestAttachment>();
    public ICollection<Quote> Quotes { get; set; } = new List<Quote>();
}

public class CustomRequestAttachment : Entity
{
    public Guid RequestId { get; set; }
    public CustomRequest? Request { get; set; }
    public string FileKey { get; set; } = null!;    // object storage key
    public string FileName { get; set; } = null!;
    public string ContentType { get; set; } = null!;
    public long SizeBytes { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
}

public class Quote : Entity
{
    public Guid CustomRequestId { get; set; }
    public CustomRequest? CustomRequest { get; set; }
    public int Version { get; set; } = 1;
    public long AmountKobo { get; set; }
    public long DepositKobo { get; set; }
    public int TimelineWeeks { get; set; }
    public string? Notes { get; set; }
    public DateTimeOffset? ValidUntil { get; set; }
    public string Status { get; set; } = "sent";    // sent | accepted | declined | expired | superseded
    public Guid SentById { get; set; }
    public User? SentBy { get; set; }
    public DateTimeOffset SentAt { get; set; }
    public DateTimeOffset? RespondedAt { get; set; }
}
