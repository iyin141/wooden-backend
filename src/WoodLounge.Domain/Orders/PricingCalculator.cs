namespace WoodLounge.Domain.Orders;

/// <summary>
/// Server-side pricing (spec §5.3). Never trust client-supplied prices — recompute from the DB.
/// unit_price = base_price + Σ option price_delta
/// subtotal   = Σ unit_price × qty
/// total      = subtotal + delivery_fee − discount
/// All amounts are integer kobo.
/// </summary>
public static class PricingCalculator
{
    public static long UnitPrice(long basePriceKobo, IEnumerable<long> optionDeltasKobo)
        => basePriceKobo + optionDeltasKobo.Sum();

    public static long LineTotal(long unitPriceKobo, int quantity)
    {
        if (quantity <= 0) throw new ArgumentOutOfRangeException(nameof(quantity), "quantity must be > 0");
        return unitPriceKobo * quantity;
    }

    public static long Subtotal(IEnumerable<long> lineTotalsKobo)
        => lineTotalsKobo.Sum();

    public static long Total(long subtotalKobo, long deliveryFeeKobo, long discountKobo)
    {
        var total = subtotalKobo + deliveryFeeKobo - discountKobo;
        return total < 0 ? 0 : total;
    }

    public static long BalanceDue(long totalKobo, long amountPaidKobo)
        => totalKobo - amountPaidKobo;

    /// <summary>Default deposit from a percentage (e.g. site_settings.deposit_default_percent = 50).</summary>
    public static long DepositFromPercent(long amountKobo, int percent)
    {
        if (percent is < 0 or > 100) throw new ArgumentOutOfRangeException(nameof(percent));
        return amountKobo * percent / 100;
    }
}
