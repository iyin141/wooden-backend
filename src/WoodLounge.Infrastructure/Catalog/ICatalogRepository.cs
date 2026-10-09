using WoodLounge.Application.Catalog;

namespace WoodLounge.Infrastructure.Catalog;

public interface ICatalogRepository
{
    Task<PagedResult<ProductDto>> GetProductsAsync(ProductsQuery query, CancellationToken ct = default);
    Task<ProductDto?> GetProductBySlugAsync(string slug, CancellationToken ct = default);
    Task<List<CategoryDto>> GetCategoriesAsync(CancellationToken ct = default);
    Task<List<RoomDto>> GetRoomsAsync(CancellationToken ct = default);
    Task<List<ShowroomDto>> GetShowroomsAsync(CancellationToken ct = default);
    Task<DeliveryRateDto?> GetDeliveryRateAsync(string state, CancellationToken ct = default);
    Task<List<DeliveryRateDto>> GetDeliveryRatesAsync(CancellationToken ct = default);
    Task<PublicSettingsDto> GetPublicSettingsAsync(CancellationToken ct = default);
}
