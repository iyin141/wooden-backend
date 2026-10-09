namespace WoodLounge.Application.Admin;

public class AdminService : IAdminService
{
    public Task<DashboardStats> GetDashboardStatsAsync()
    {
        return Task.FromResult(new DashboardStats(
            150,
            12,
            45000000,
            8,
            25
        ));
    }

    public Task<List<OrderSummary>> GetOrdersAsync()
    {
        // In a real implementation, this would query the database
        return Task.FromResult(new List<OrderSummary>());
    }

    public Task<List<CustomRequestSummary>> GetCustomRequestsAsync()
    {
        // In a real implementation, this would query the database
        return Task.FromResult(new List<CustomRequestSummary>());
    }

    public Task<List<VisitSummary>> GetVisitsAsync()
    {
        // In a real implementation, this would query the database
        return Task.FromResult(new List<VisitSummary>());
    }
}
