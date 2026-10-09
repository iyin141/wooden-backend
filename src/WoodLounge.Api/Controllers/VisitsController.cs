using Microsoft.AspNetCore.Mvc;
using WoodLounge.Application.Visits;

namespace WoodLounge.Api.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public class VisitsController : ControllerBase
{
    private readonly IVisitsService _visitsService;

    public VisitsController(IVisitsService visitsService)
    {
        _visitsService = visitsService;
    }

    [HttpGet("availability")]
    public async Task<ActionResult<List<VisitAvailabilityResponse>>> GetAvailability([FromQuery] int daysAhead = 14)
    {
        var availability = await _visitsService.GetAvailabilityAsync(daysAhead);
        return Ok(availability);
    }

    [HttpPost("book")]
    public async Task<ActionResult<BookVisitResponse>> BookVisit([FromBody] BookVisitRequest request)
    {
        var booking = await _visitsService.BookVisitAsync(request);
        return Ok(booking);
    }
}
