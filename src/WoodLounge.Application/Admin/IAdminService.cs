namespace WoodLounge.Application.Admin;

public interface IAdminService
{
    Task<DashboardStats> GetDashboardStatsAsync();
    Task<List<OrderSummary>> GetOrdersAsync();
    Task<List<CustomRequestSummary>> GetCustomRequestsAsync();
    Task<List<VisitSummary>> GetVisitsAsync();
}
