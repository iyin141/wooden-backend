using Microsoft.AspNetCore.Mvc;
using WoodLounge.Application.Admin;

namespace WoodLounge.Api.Controllers;

[ApiController]
[Route("api/v1/admin")]
public class AdminController : ControllerBase
{
    private readonly IAdminService _adminService;

    public AdminController(IAdminService adminService)
    {
        _adminService = adminService;
    }

    [HttpGet("dashboard")]
    public async Task<ActionResult<DashboardStats>> GetDashboardStats()
    {
        var stats = await _adminService.GetDashboardStatsAsync();
        return Ok(stats);
    }

    [HttpGet("orders")]
    public async Task<ActionResult<List<OrderSummary>>> GetOrders()
    {
        var orders = await _adminService.GetOrdersAsync();
        return Ok(orders);
    }

    [HttpGet("custom-requests")]
    public async Task<ActionResult<List<CustomRequestSummary>>> GetCustomRequests()
    {
        var requests = await _adminService.GetCustomRequestsAsync();
        return Ok(requests);
    }

    [HttpGet("visits")]
    public async Task<ActionResult<List<VisitSummary>>> GetVisits()
    {
        var visits = await _adminService.GetVisitsAsync();
        return Ok(visits);
    }
}
