using Microsoft.Extensions.DependencyInjection;
using WoodLounge.Application.Catalog;
using WoodLounge.Application.Auth;
using WoodLounge.Application.Checkout;
using WoodLounge.Application.Visits;
using WoodLounge.Application.CustomOrders;
using WoodLounge.Application.Admin;

namespace WoodLounge.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<ICatalogService, CatalogService>();
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<ICheckoutService, CheckoutService>();
        services.AddScoped<IVisitsService, VisitsService>();
        services.AddScoped<ICustomOrdersService, CustomOrdersService>();
        services.AddScoped<IAdminService, AdminService>();

        return services;
    }
}
