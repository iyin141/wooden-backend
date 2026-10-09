using WoodLounge.Domain.Common;
using WoodLounge.Domain.CustomOrders;
using WoodLounge.Domain.Identity;
using WoodLounge.Domain.Visits;

namespace WoodLounge.Domain.Messaging;

public class MessageThread : Entity
{
    public string Kind { get; set; } = null!;       // custom_request | site_visit
    public Guid? CustomRequestId { get; set; }
    public CustomRequest? CustomRequest { get; set; }
    public Guid? SiteVisitId { get; set; }
    public SiteVisitBooking? SiteVisit { get; set; }
    public Guid UserId { get; set; }
    public User? User { get; set; }
    public DateTimeOffset LastMessageAt { get; set; }
    public int CustomerUnreadCount { get; set; }
    public int AdminUnreadCount { get; set; }
    public DateTimeOffset CreatedAt { get; set; }

    public ICollection<Message> Messages { get; set; } = new List<Message>();
}

public class Message : Entity
{
    public Guid ThreadId { get; set; }
    public MessageThread? Thread { get; set; }
    public Guid? SenderUserId { get; set; }
    public User? SenderUser { get; set; }
    public string SenderRole { get; set; } = null!; // customer | admin | system
    public string Kind { get; set; } = "text";      // text | quote | status_change | system
    public string Body { get; set; } = null!;
    public Guid? QuoteId { get; set; }
    public Quote? Quote { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? ReadAt { get; set; }

    public ICollection<MessageAttachment> Attachments { get; set; } = new List<MessageAttachment>();
}

public class MessageAttachment : Entity
{
    public Guid MessageId { get; set; }
    public Message? Message { get; set; }
    public string FileKey { get; set; } = null!;
    public string FileName { get; set; } = null!;
    public string ContentType { get; set; } = null!;
    public long SizeBytes { get; set; }
}
