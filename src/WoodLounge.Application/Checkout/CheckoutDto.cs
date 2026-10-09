namespace WoodLounge.Application.Checkout;

public record CheckoutQuoteRequest(
    List<CheckoutQuoteItemRequest> Items,
    string? DeliveryState
);

public record CheckoutQuoteItemRequest(
    string ProductId,
    int Quantity,
    string? SelectedVariantId
);

public record CheckoutQuoteResponse(
    long SubtotalKobo,
    long DeliveryKobo,
    long TotalKobo,
    string DeliveryEta,
    List<CheckoutQuoteItemResponse> Items
);

public record CheckoutQuoteItemResponse(
    string ProductId,
    string ProductName,
    int Quantity,
    long UnitPriceKobo,
    long LineTotalKobo
);

public record CreateOrderRequest(
    List<CreateOrderItemRequest> Items,
    string CustomerEmail,
    string CustomerPhone,
    string CustomerFullName,
    string AddressLine1,
    string? AddressLine2,
    string City,
    string State,
    string? Notes
);

public record CreateOrderItemRequest(
    string ProductId,
    int Quantity,
    string? SelectedVariantId
);

public record CreateOrderResponse(
    string OrderNumber,
    long TotalKobo,
    string PaymentReference
);

public record VerifyPaymentRequest(
    string Reference
);

public record VerifyPaymentResponse(
    bool Success,
    string? OrderNumber,
    string? Status
);
