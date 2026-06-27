using Application.Admin.Products.ProductSkus.Types;
using Application.ProductSkus.Types;
using Domain.Shared.ValueObjects;

namespace Application.ProductSkus;

public interface IProductSkusReadRepository
{
    Task<KeysetPaginated<ProductSkuListItemResponse, Guid>> GetProductSkusAsync(
        ProductSkusFilters filters,
        KeysetPagination<Guid> keysetPagination,
        CancellationToken ct = default
    );

    Task<KeysetPaginated<AdminProductSkuResponse, Guid>> GetAdminProductSkusAsync(
        AdminProductSkusFilters filters,
        KeysetPagination<Guid> keysetPagination,
        CancellationToken ct = default
    );

    Task<ProductSkuResponse?> GetProductSkuByIdAsync(
        ProductSkuId productId,
        CancellationToken ct = default
    );

    Task<AdminProductSkuResponse?> GetAdminProductSkuByIdAsync(
        ProductSkuId productId,
        CancellationToken ct = default
    );

    Task<IReadOnlyList<ProductSkuResponse>> GetVariants(
        ProductId productId,
        CancellationToken ct = default
    );

    Task<ProductSkusFilterOptions> GetProductSkusFilterOptions(CancellationToken ct = default);

    Task<bool> ExistsBySkuAsync(ProductSkuSku sku, CancellationToken ct = default);
    Task<bool> ExistsByProductSizeColorAsync(
        ProductId productId,
        PositiveInt size,
        ProductSkuColor color,
        CancellationToken ct = default
    );
}
