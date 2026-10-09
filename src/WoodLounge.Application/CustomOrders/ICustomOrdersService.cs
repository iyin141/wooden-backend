namespace WoodLounge.Application.CustomOrders;

public interface ICustomOrdersService
{
    Task<CreateCustomRequestResponse> CreateRequestAsync(CreateCustomRequestRequest request);
    Task<List<CustomRequestResponse>> GetRequestsAsync(string customerPhone);
}
