using Application.Admin.Products.Types;

namespace Application.Admin.Products;

public interface IProductReadRepository
{
    Task<ProductFilterOptions> GetProductsFilterOptions(CancellationToken ct = default);

    Task<KeysetPaginated<AdminProductResponse, Guid>> GetProductsAsync(
        ProductFilters filters,
        KeysetPagination<Guid> keysetPagination,
        CancellationToken ct = default
    );
}
