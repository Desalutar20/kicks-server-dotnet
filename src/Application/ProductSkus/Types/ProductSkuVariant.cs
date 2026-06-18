using Domain.Shared.ValueObjects;

namespace Application.ProductSkus.Types;

public sealed record ProductSkuVariant(
    PositiveInt Size,
    ProductSkuId ProductSkuId,
    bool InStock,
    List<ProductSkuVariantColor> Colors
);
