namespace Application.ProductSkus.Types;

public sealed record ProductSkuVariant(
    int Size,
    Guid ProductSkuId,
    bool InStock,
    List<ProductSkuVariantColor> Colors
);
