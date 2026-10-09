namespace WoodLounge.Application.CustomOrders;

public class CustomOrdersService : ICustomOrdersService
{
    public Task<CreateCustomRequestResponse> CreateRequestAsync(CreateCustomRequestRequest request)
    {
        var reference = $"REQ{DateTime.UtcNow:yyyyMMddHHmmss}{Random.Shared.Next(1000, 9999)}";

        return Task.FromResult(new CreateCustomRequestResponse(
            reference,
            "pending_review"
        ));
    }

    public Task<List<CustomRequestResponse>> GetRequestsAsync(string customerPhone)
    {
        // In a real implementation, this would query the database
        // For now, return empty list
        return Task.FromResult(new List<CustomRequestResponse>());
    }
}
