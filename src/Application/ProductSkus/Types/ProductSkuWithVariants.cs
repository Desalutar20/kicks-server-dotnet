namespace Application.ProductSkus.Types;

public sealed record ProductSkuWithVariants(
    ProductSku ProductSku,
    List<ProductSkuVariant> Variants
);
