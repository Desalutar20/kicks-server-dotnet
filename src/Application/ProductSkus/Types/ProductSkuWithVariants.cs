namespace Application.ProductSkus.Types;

public sealed record ProductSkuWithVariants(
    ProductSkuResponse ProductSku,
    List<ProductSkuVariant> Variants
);
