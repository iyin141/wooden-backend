namespace WoodLounge.Application.Admin;

public record DashboardStats(
    int TotalOrders,
    int PendingOrders,
    long TotalRevenueKobo,
    int CustomRequests,
    int VisitsBooked
);

public record OrderSummary(
    string OrderNumber,
    string CustomerName,
    string Status,
    long TotalKobo,
    DateTime CreatedAt
);

public record CustomRequestSummary(
    string Reference,
    string CustomerName,
    string Category,
    string Status,
    DateTime CreatedAt
);

public record VisitSummary(
    string Reference,
    string CustomerName,
    DateTime Date,
    string Time,
    string Status
);
