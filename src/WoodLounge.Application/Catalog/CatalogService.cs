namespace WoodLounge.Application.Catalog;

public class CatalogService(ICatalogRepository repository) : ICatalogService
{
    public Task<PagedResult<ProductDto>> GetProductsAsync(ProductsQuery query, CancellationToken ct = default)
    {
        return repository.GetProductsAsync(query, ct);
    }

    public Task<ProductDto?> GetProductBySlugAsync(string slug, CancellationToken ct = default)
    {
        return repository.GetProductBySlugAsync(slug, ct);
    }

    public Task<List<CategoryDto>> GetCategoriesAsync(CancellationToken ct = default)
    {
        return repository.GetCategoriesAsync(ct);
    }

    public Task<List<RoomDto>> GetRoomsAsync(CancellationToken ct = default)
    {
        return repository.GetRoomsAsync(ct);
    }

    public Task<List<ShowroomDto>> GetShowroomsAsync(CancellationToken ct = default)
    {
        return repository.GetShowroomsAsync(ct);
    }

    public Task<DeliveryRateDto?> GetDeliveryRateAsync(string state, CancellationToken ct = default)
    {
        return repository.GetDeliveryRateAsync(state, ct);
    }

    public Task<List<DeliveryRateDto>> GetDeliveryRatesAsync(CancellationToken ct = default)
    {
        return repository.GetDeliveryRatesAsync(ct);
    }

    public Task<PublicSettingsDto> GetPublicSettingsAsync(CancellationToken ct = default)
    {
        return repository.GetPublicSettingsAsync(ct);
    }
}
