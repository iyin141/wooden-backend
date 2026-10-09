using Microsoft.AspNetCore.Mvc;
using WoodLounge.Application.Checkout;

namespace WoodLounge.Api.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public class CheckoutController : ControllerBase
{
    private readonly ICheckoutService _checkoutService;

    public CheckoutController(ICheckoutService checkoutService)
    {
        _checkoutService = checkoutService;
    }

    [HttpPost("quote")]
    public async Task<ActionResult<CheckoutQuoteResponse>> GetQuote([FromBody] CheckoutQuoteRequest request)
    {
        var quote = await _checkoutService.GetQuoteAsync(request);
        return Ok(quote);
    }

    [HttpPost("orders")]
    public async Task<ActionResult<CreateOrderResponse>> CreateOrder([FromBody] CreateOrderRequest request)
    {
        var order = await _checkoutService.CreateOrderAsync(request);
        return Ok(order);
    }

    [HttpPost("payments/verify/{reference}")]
    public async Task<ActionResult<VerifyPaymentResponse>> VerifyPayment(string reference)
    {
        var result = await _checkoutService.VerifyPaymentAsync(reference);
        return Ok(result);
    }
}
