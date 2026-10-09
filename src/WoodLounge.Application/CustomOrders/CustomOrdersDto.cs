namespace WoodLounge.Application.CustomOrders;

public record CreateCustomRequestRequest(
    string CustomerName,
    string CustomerPhone,
    string CustomerEmail,
    string Category,
    string Description,
    string? BudgetRange,
    List<string>? ImageUrls,
    string? Notes
);

public record CreateCustomRequestResponse(
    string Reference,
    string Status
);

public record CustomRequestResponse(
    string Reference,
    string CustomerName,
    string Category,
    string Description,
    string Status,
    DateTime CreatedAt
);
