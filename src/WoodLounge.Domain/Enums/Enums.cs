namespace WoodLounge.Domain.Enums;

// Enum-like fields are stored as text with CHECK constraints (see spec §4).
// String values below must match the SQL CHECK lists exactly.

public static class Roles
{
    public const string Customer = "customer";
    public const string Admin = "admin";
    public const string SuperAdmin = "super_admin";
}

public enum OtpPurpose
{
    Login,
    VerifyPhone,
}

public enum ProductStatus
{
    Draft,
    Active,
    Archived,
}

public enum ProductImageKind
{
    Studio,
    Lifestyle,
    CustomerHome,
}

public enum ProductOptionInputType
{
    Select,
    Swatch,
    Radio,
}

public enum VisitType
{
    SiteInspection,
    OnlineVisit,
    ShowroomVisit,
}

public enum VisitStatus
{
    Pending,
    Confirmed,
    RescheduleProposed,
    Declined,
    Completed,
    Cancelled,
}

public enum CustomRequestTier
{
    ModifyExisting,
    FullyBespoke,
    FullRoom,
}

public enum CustomRequestStatus
{
    Submitted,
    InDiscussion,
    Quoted,
    QuoteAccepted,
    QuoteDeclined,
    Closed,
    Cancelled,
}

public enum QuoteStatus
{
    Sent,
    Accepted,
    Declined,
    Expired,
    Superseded,
}

public enum MessageThreadKind
{
    CustomRequest,
    SiteVisit,
}

public enum MessageSenderRole
{
    Customer,
    Admin,
    System,
}

public enum MessageKind
{
    Text,
    Quote,
    StatusChange,
    System,
}

public enum OrderKind
{
    Premade,
    Custom,
}

public enum OrderStatus
{
    // premade chain
    PendingPayment,
    Paid,
    Processing,
    Ready,
    OutForDelivery,
    Delivered,
    // custom chain (Ready/OutForDelivery/Delivered shared with premade)
    QuoteAccepted,
    DepositPaid,
    InProduction,
    // terminal
    Cancelled,
    Refunded,
}

public enum PaymentKind
{
    Full,
    Deposit,
    Balance,
}

public enum PaymentStatus
{
    Initialized,
    Pending,
    Success,
    Failed,
    Abandoned,
    Refunded,
}

public enum NotificationChannel
{
    Email,
    Whatsapp,
    Sms,
}

public enum NotificationStatus
{
    Queued,
    Sent,
    Failed,
}
