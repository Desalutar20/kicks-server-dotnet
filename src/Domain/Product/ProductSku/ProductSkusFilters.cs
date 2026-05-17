using Domain.Shared;

namespace Domain.Product.ProductSku;

public sealed record ProductSkusFilters(
    bool? InStock,
    PositiveInt? MinPrice,
    PositiveInt? MaxPrice,
    PositiveInt? MinSalePrice,
    PositiveInt? MaxSalePrice,
    PositiveInt? Size,
    ProductSkuColor? Color,
    ProductSkuSku? Sku
);
