using WoodLounge.Domain.Common;
using WoodLounge.Domain.Orders;

namespace WoodLounge.Domain.Payments;

public class Payment : AuditedEntity
{
    public Guid OrderId { get; set; }
    public Order? Order { get; set; }
    public string Kind { get; set; } = null!;        // full | deposit | balance
    public string Provider { get; set; } = "paystack";
    public string Reference { get; set; } = null!;   // our reference sent to Paystack (unique)
    public long? PaystackTransactionId { get; set; }
    public long AmountKobo { get; set; }
    public string Currency { get; set; } = "NGN";
    public string Status { get; set; } = "initialized"; // initialized | pending | success | failed | abandoned | refunded
    public string? Channel { get; set; }             // card, bank, ussd, bank_transfer...
    public long? FeesKobo { get; set; }
    public string CustomerEmail { get; set; } = null!; // citext
    public string? AuthorizationUrl { get; set; }
    public string? AccessCode { get; set; }
    public DateTimeOffset? PaidAt { get; set; }
    public string? FailureReason { get; set; }
    public string? RawJson { get; set; }             // jsonb, last provider payload
}

public class PaymentWebhookEvent : Entity
{
    public string Provider { get; set; } = "paystack";
    public string EventId { get; set; } = null!;     // Paystack event id, or hash of body if absent
    public string EventType { get; set; } = null!;
    public bool SignatureValid { get; set; }
    public string PayloadJson { get; set; } = null!; // jsonb
    public DateTimeOffset? ProcessedAt { get; set; }
    public string? Error { get; set; }
    public DateTimeOffset ReceivedAt { get; set; }
}
