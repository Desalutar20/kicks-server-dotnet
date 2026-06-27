using Domain.Shared.Pagination;
using Domain.Shared.ValueObjects;

namespace Domain.Products.ProductSkus;

public interface IProductSkusRepository
{
    Task<KeysetPaginated<ProductSku, ProductSkuId>> GetAdminProductSkusAsync(
        AdminProductSkusFilters filters,
        KeysetPagination<ProductSkuId> keysetPagination,
        bool trackChanges,
        CancellationToken ct = default
    );

    Task<ProductSku?> GetProductSkuByIdAsync(
        ProductSkuId productId,
        bool trackChanges,
        CancellationToken ct = default
    );

    Task BulkIncrementQuantityAsync(
        IEnumerable<(ProductSkuId Id, PositiveInt Quantity)> data,
        CancellationToken ct = default
    );

    void CreateProductSku(ProductSku product);
    void UpdateProductSku(ProductSku product);
    void DeleteProductSku(ProductSku product);
}
