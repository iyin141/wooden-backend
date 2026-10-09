using Microsoft.AspNetCore.Mvc;
using WoodLounge.Application.Catalog;

namespace WoodLounge.Api.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public class CatalogController : ControllerBase
{
    private readonly ICatalogService _catalogService;

    public CatalogController(ICatalogService catalogService)
    {
        _catalogService = catalogService;
    }

    [HttpGet("products")]
    public async Task<ActionResult<PagedResult<ProductDto>>> GetProducts([FromQuery] ProductsQuery query)
    {
        var result = await _catalogService.GetProductsAsync(query);
        return Ok(result);
    }

    [HttpGet("products/{slug}")]
    public async Task<ActionResult<ProductDto>> GetProductBySlug(string slug)
    {
        var product = await _catalogService.GetProductBySlugAsync(slug);
        if (product == null) return NotFound();
        return Ok(product);
    }

    [HttpGet("categories")]
    public async Task<ActionResult<List<CategoryDto>>> GetCategories()
    {
        var categories = await _catalogService.GetCategoriesAsync();
        return Ok(categories);
    }

    [HttpGet("rooms")]
    public async Task<ActionResult<List<RoomDto>>> GetRooms()
    {
        var rooms = await _catalogService.GetRoomsAsync();
        return Ok(rooms);
    }

    [HttpGet("showrooms")]
    public async Task<ActionResult<List<ShowroomDto>>> GetShowrooms()
    {
        var showrooms = await _catalogService.GetShowroomsAsync();
        return Ok(showrooms);
    }

    [HttpGet("delivery-rates")]
    public async Task<ActionResult<List<DeliveryRateDto>>> GetDeliveryRates([FromQuery] string? state)
    {
        if (!string.IsNullOrWhiteSpace(state))
        {
            var rate = await _catalogService.GetDeliveryRateAsync(state);
            if (rate == null) return NotFound();
            return Ok(new List<DeliveryRateDto> { rate });
        }

        var rates = await _catalogService.GetDeliveryRatesAsync();
        return Ok(rates);
    }

    [HttpGet("settings/public")]
    public async Task<ActionResult<PublicSettingsDto>> GetPublicSettings()
    {
        var settings = await _catalogService.GetPublicSettingsAsync();
        return Ok(settings);
    }
}
