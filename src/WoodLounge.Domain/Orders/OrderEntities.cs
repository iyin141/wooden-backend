using WoodLounge.Domain.Common;
using WoodLounge.Domain.CustomOrders;
using WoodLounge.Domain.Catalog;
using WoodLounge.Domain.Identity;

namespace WoodLounge.Domain.Orders;

public class Order : AuditedEntity
{
    public string OrderNumber { get; set; } = null!; // e.g. WL-10001 (from sequence)
    public Guid? UserId { get; set; }                // null for guest checkout
    public User? User { get; set; }
    public string? GuestEmail { get; set; }          // citext
    public string? GuestPhone { get; set; }
    public string CustomerName { get; set; } = null!;
    public string Kind { get; set; } = null!;        // premade | custom
    public Guid? CustomRequestId { get; set; }
    public CustomRequest? CustomRequest { get; set; }
    public Guid? QuoteId { get; set; }
    public Quote? Quote { get; set; }
    public string Status { get; set; } = "pending_payment";
    public long SubtotalKobo { get; set; }
    public long DeliveryFeeKobo { get; set; }
    public long DiscountKobo { get; set; }
    public long TotalKobo { get; set; }
    public long AmountPaidKobo { get; set; }
    // balance_due_kobo is a generated column (total - amount_paid); mapped read-only.
    public long BalanceDueKobo { get; set; }
    public string ShipName { get; set; } = null!;
    public string ShipPhone { get; set; } = null!;
    public string ShipLine1 { get; set; } = null!;
    public string? ShipLine2 { get; set; }
    public string ShipCity { get; set; } = null!;
    public string ShipState { get; set; } = null!;
    public string? ShipNotes { get; set; }
    public DateOnly? EstimatedDeliveryDate { get; set; }
    public string? AdminNote { get; set; }

    public ICollection<OrderItem> Items { get; set; } = new List<OrderItem>();
    public ICollection<OrderStatusHistory> StatusHistory { get; set; } = new List<OrderStatusHistory>();
    public ICollection<Payments.Payment> Payments { get; set; } = new List<Payments.Payment>();
}

public class OrderItem : Entity
{
    public Guid OrderId { get; set; }
    public Order? Order { get; set; }
    public Guid? ProductId { get; set; }
    public Product? Product { get; set; }
    public string NameSnapshot { get; set; } = null!;
    public string? SkuSnapshot { get; set; }
    public string? ImageUrlSnapshot { get; set; }
    public long UnitPriceKobo { get; set; }          // base + option deltas, computed server-side
    public int Quantity { get; set; }
    public string SelectedOptionsJson { get; set; } = "[]"; // jsonb [{group, value, price_delta_kobo}]
    public long LineTotalKobo { get; set; }
}

public class OrderStatusHistory : Entity
{
    public Guid OrderId { get; set; }
    public Order? Order { get; set; }
    public string Status { get; set; } = null!;
    public string? Note { get; set; }
    public bool NotifyCustomer { get; set; } = true;
    public Guid? ChangedById { get; set; }           // null = system (e.g. payment webhook)
    public User? ChangedBy { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
}
