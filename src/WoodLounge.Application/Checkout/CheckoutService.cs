using WoodLounge.Application.Catalog;

namespace WoodLounge.Application.Checkout;

public class CheckoutService : ICheckoutService
{
    private readonly ICatalogRepository _catalogRepository;

    public CheckoutService(ICatalogRepository catalogRepository)
    {
        _catalogRepository = catalogRepository;
    }

    public async Task<CheckoutQuoteResponse> GetQuoteAsync(CheckoutQuoteRequest request)
    {
        var items = new List<CheckoutQuoteItemResponse>();
        long subtotalKobo = 0;

        foreach (var itemRequest in request.Items)
        {
            var product = await _catalogRepository.GetByIdAsync(Guid.Parse(itemRequest.ProductId));
            if (product == null)
                throw new ArgumentException($"Product {itemRequest.ProductId} not found");

            var unitPriceKobo = product.BasePriceKobo;
            var lineTotalKobo = unitPriceKobo * itemRequest.Quantity;

            items.Add(new CheckoutQuoteItemResponse(
                product.Id.ToString(),
                product.Name,
                itemRequest.Quantity,
                unitPriceKobo,
                lineTotalKobo
            ));

            subtotalKobo += lineTotalKobo;
        }

        // Calculate delivery (simplified - in production, use delivery rates table)
        var deliveryKobo = CalculateDelivery(subtotalKobo, request.DeliveryState);
        var totalKobo = subtotalKobo + deliveryKobo;
        var deliveryEta = EstimateDeliveryEta(request.DeliveryState);

        return new CheckoutQuoteResponse(
            subtotalKobo,
            deliveryKobo,
            totalKobo,
            deliveryEta,
            items
        );
    }

    public async Task<CreateOrderResponse> CreateOrderAsync(CreateOrderRequest request)
    {
        // Validate and calculate total server-side
        var quoteRequest = new CheckoutQuoteRequest(
            request.Items.Select(i => new CheckoutQuoteItemRequest(
                i.ProductId,
                i.Quantity,
                i.SelectedVariantId
            )).ToList(),
            request.State
        );

        var quote = await GetQuoteAsync(quoteRequest);

        // In a real implementation, this would:
        // 1. Create the order in the database
        // 2. Initialize Paystack payment
        // 3. Return the payment reference

        var orderNumber = GenerateOrderNumber();
        var paymentReference = $"PAY_{orderNumber}_{Guid.NewGuid().ToString("N")[..8].ToUpper()}";

        return new CreateOrderResponse(
            orderNumber,
            quote.TotalKobo,
            paymentReference
        );
    }

    public async Task<VerifyPaymentResponse> VerifyPaymentAsync(string reference)
    {
        // In a real implementation, this would:
        // 1. Call Paystack API to verify the transaction
        // 2. Update the order status in the database
        // 3. Return the verification result

        return new VerifyPaymentResponse(
            Success: true,
            OrderNumber: reference.Split('_')[1],
            Status: "paid"
        );
    }

    private long CalculateDelivery(long subtotalKobo, string? state)
    {
        // Simplified delivery calculation
        // In production, use the delivery_rates table
        return subtotalKobo >= 5000000 ? 0 : 500000; // Free delivery over ₦50,000
    }

    private string EstimateDeliveryEta(string? state)
    {
        // Simplified ETA estimation
        return state?.ToLowerInvariant() switch
        {
            "lagos" => "2-3 business days",
            "abuja" => "3-5 business days",
            _ => "5-7 business days"
        };
    }

    private string GenerateOrderNumber()
    {
        // In production, use the database sequence
        return $"WL{DateTime.UtcNow:yyyyMMdd}{Random.Shared.Next(1000, 9999)}";
    }
}
