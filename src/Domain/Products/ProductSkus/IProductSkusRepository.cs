using Domain.Shared;
using Domain.Shared.Pagination;

namespace Domain.Products.ProductSkus;

public interface IProductSkusRepository
{
    Task<KeysetPaginated<ProductSku, ProductSkuId>> GetProductSkusAsync(
        ProductSkusFilters filters,
        KeysetPagination<ProductSkuId> keysetPagination,
        bool trackChanges,
        CancellationToken ct = default
    );

    Task<IEnumerable<ProductSku>> GetVariants(
        ProductId productId,
        bool trackChanges,
        CancellationToken ct = default
    );

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

    Task<ProductSkusFilterOptions> GetProductSkusFilterOptions(
        bool trackChanges,
        CancellationToken ct = default
    );

    Task<bool> ExistsBySkuAsync(ProductSkuSku sku, CancellationToken ct = default);
    Task<bool> ExistsByProductSizeColorAsync(
        ProductId productId,
        PositiveInt size,
        ProductSkuColor color,
        CancellationToken ct = default
    );

    void CreateProductSku(ProductSku product);
    void UpdateProductSku(ProductSku product);
    void DeleteProductSku(ProductSku product);
}
