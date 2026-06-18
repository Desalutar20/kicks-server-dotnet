using Domain.Shared;
using Domain.Shared.ValueObjects;

namespace Domain.Products.ProductSkus;

public sealed record AdminProductSkusFilters(
    bool? InStock,
    Money? MinPrice,
    Money? MaxPrice,
    Money? MinSalePrice,
    Money? MaxSalePrice,
    PositiveInt? Size,
    ProductSkuColor? Color,
    ProductSkuSku? Sku
);
