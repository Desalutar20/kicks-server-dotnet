using Domain.Shared;

namespace Domain.Products.ProductSkus;

public sealed record AdminProductSkusFilters(
    bool? InStock,
    PositiveInt? MinPrice,
    PositiveInt? MaxPrice,
    PositiveInt? MinSalePrice,
    PositiveInt? MaxSalePrice,
    PositiveInt? Size,
    ProductSkuColor? Color,
    ProductSkuSku? Sku
);
