namespace WoodLounge.Application.Checkout;

public interface ICheckoutService
{
    Task<CheckoutQuoteResponse> GetQuoteAsync(CheckoutQuoteRequest request);
    Task<CreateOrderResponse> CreateOrderAsync(CreateOrderRequest request);
    Task<VerifyPaymentResponse> VerifyPaymentAsync(string reference);
}
