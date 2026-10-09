using Microsoft.AspNetCore.Mvc;
using WoodLounge.Application.CustomOrders;

namespace WoodLounge.Api.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public class CustomOrdersController : ControllerBase
{
    private readonly ICustomOrdersService _customOrdersService;

    public CustomOrdersController(ICustomOrdersService customOrdersService)
    {
        _customOrdersService = customOrdersService;
    }

    [HttpPost]
    public async Task<ActionResult<CreateCustomRequestResponse>> CreateRequest([FromBody] CreateCustomRequestRequest request)
    {
        var response = await _customOrdersService.CreateRequestAsync(request);
        return Ok(response);
    }

    [HttpGet]
    public async Task<ActionResult<List<CustomRequestResponse>>> GetRequests([FromQuery] string customerPhone)
    {
        var requests = await _customOrdersService.GetRequestsAsync(customerPhone);
        return Ok(requests);
    }
}
