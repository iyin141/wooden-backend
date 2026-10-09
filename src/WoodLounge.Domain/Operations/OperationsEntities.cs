using WoodLounge.Domain.Common;
using WoodLounge.Domain.Identity;

namespace WoodLounge.Domain.Operations;

public class Notification : Entity
{
    public Guid? UserId { get; set; }
    public User? User { get; set; }
    public string Channel { get; set; } = null!;     // email | whatsapp | sms
    public string ToAddress { get; set; } = null!;
    public string Template { get; set; } = null!;    // quote_sent, visit_confirmed, order_status_changed, otp...
    public string PayloadJson { get; set; } = "{}";  // jsonb
    public string Status { get; set; } = "queued";   // queued | sent | failed
    public int Attempts { get; set; }
    public string? LastError { get; set; }
    public DateTimeOffset ScheduledAt { get; set; }
    public DateTimeOffset? SentAt { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
}

public class AuditLog
{
    public long Id { get; set; }                     // bigint identity
    public Guid? ActorUserId { get; set; }
    public User? ActorUser { get; set; }
    public string Action { get; set; } = null!;      // order.status_changed, quote.sent, product.updated...
    public string EntityType { get; set; } = null!;
    public string EntityId { get; set; } = null!;
    public string? BeforeJson { get; set; }          // jsonb
    public string? AfterJson { get; set; }           // jsonb
    public System.Net.IPAddress? Ip { get; set; }    // inet
    public DateTimeOffset CreatedAt { get; set; }
}

public class SiteSetting
{
    public string Key { get; set; } = null!;         // PK: whatsapp_number, deposit_default_percent...
    public string ValueJson { get; set; } = null!;   // jsonb
    public Guid? UpdatedById { get; set; }
    public User? UpdatedBy { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
}
