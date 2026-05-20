namespace Application.ProductSkus.Types;

public sealed record ProductSkuVariantColor(
    ProductSkuColor Color,
    ProductSkuId ProductSkuId,
    bool InStock
);
