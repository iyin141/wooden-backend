namespace WoodLounge.Domain.Orders;

/// <summary>
/// Allowed order status transitions (spec §5.3). Enforced in the Domain layer and unit-tested.
/// Values are the snake_case strings stored in the DB.
/// </summary>
public static class OrderStatusTransitions
{
    public const string PendingPayment = "pending_payment";
    public const string Paid = "paid";
    public const string Processing = "processing";
    public const string Ready = "ready";
    public const string OutForDelivery = "out_for_delivery";
    public const string Delivered = "delivered";
    public const string QuoteAccepted = "quote_accepted";
    public const string DepositPaid = "deposit_paid";
    public const string InProduction = "in_production";
    public const string Cancelled = "cancelled";
    public const string Refunded = "refunded";

    // Forward-only chains. paid/deposit_paid are set ONLY by the payment flow (see PaymentDrivenStatuses).
    private static readonly Dictionary<string, string[]> Forward = new()
    {
        // premade chain
        [PendingPayment] = new[] { Paid, Cancelled },
        [Paid] = new[] { Processing, Cancelled, Refunded },
        [Processing] = new[] { Ready, Cancelled, Refunded },
        [Ready] = new[] { OutForDelivery, Cancelled, Refunded },
        [OutForDelivery] = new[] { Delivered, Cancelled, Refunded },
        [Delivered] = new[] { Refunded },
        // custom chain
        [QuoteAccepted] = new[] { DepositPaid, Cancelled },
        [DepositPaid] = new[] { InProduction, Cancelled, Refunded },
        [InProduction] = new[] { Ready, Cancelled, Refunded },
        [Cancelled] = Array.Empty<string>(),
        [Refunded] = Array.Empty<string>(),
    };

    /// <summary>Statuses that only the payment flow may set — never the admin dropdown.</summary>
    public static readonly IReadOnlySet<string> PaymentDrivenStatuses =
        new HashSet<string> { Paid, DepositPaid };

    /// <summary>Canonical forward order of each chain, used to detect forward vs backward moves.</summary>
    private static readonly string[] PremadeChain =
        { PendingPayment, Paid, Processing, Ready, OutForDelivery, Delivered };
    private static readonly string[] CustomChain =
        { QuoteAccepted, DepositPaid, InProduction, Ready, OutForDelivery, Delivered };

    public static bool IsForwardAllowed(string from, string to)
        => Forward.TryGetValue(from, out var allowed) && allowed.Contains(to);

    public static bool IsCancelAllowed(string from)
        => from != Delivered && IsForwardAllowed(from, Cancelled);

    public static bool IsRefundAllowed(string from)
        => IsForwardAllowed(from, Refunded);

    /// <summary>
    /// True when <paramref name="to"/> sits strictly after <paramref name="from"/> in the
    /// relevant chain (a forward jump). Backward moves require super_admin + audit note.
    /// </summary>
    public static bool IsForwardMove(string from, string to)
    {
        var chain = ChainFor(from, to);
        if (chain is null) return false;
        var fi = Array.IndexOf(chain, from);
        var ti = Array.IndexOf(chain, to);
        return fi >= 0 && ti > fi;
    }

    public static bool IsBackwardMove(string from, string to)
    {
        var chain = ChainFor(from, to);
        if (chain is null) return false;
        var fi = Array.IndexOf(chain, from);
        var ti = Array.IndexOf(chain, to);
        return fi >= 0 && ti >= 0 && ti < fi;
    }

    private static string[]? ChainFor(string from, string to)
    {
        if (PremadeChain.Contains(from) || PremadeChain.Contains(to))
        {
            // custom-only statuses never appear in the premade chain
            if (CustomChain.Contains(from) && !PremadeChain.Contains(from)) return CustomChain;
            if (CustomChain.Contains(to) && !PremadeChain.Contains(to)) return CustomChain;
            return PremadeChain;
        }
        return CustomChain;
    }

    /// <summary>The valid next statuses an admin may choose (excludes payment-driven ones).</summary>
    public static IReadOnlyList<string> AdminSelectableNext(string from)
        => Forward.TryGetValue(from, out var allowed)
            ? allowed.Where(s => !PaymentDrivenStatuses.Contains(s)).ToArray()
            : Array.Empty<string>();
}
