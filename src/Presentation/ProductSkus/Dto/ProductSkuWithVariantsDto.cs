using Application.ProductSkus.Types;

namespace Presentation.ProductSkus.Dto;

public sealed record ProductSkuWithVariantsDto(
    ProductSkuDto ProductSku,
    List<ProductSkuVariantDto> Variants
);

public sealed record ProductSkuVariantDto(
    int Size,
    Guid ProductSkuId,
    bool InStock,
    List<ProductSkuVariantColorDto> Colors
);

public sealed record ProductSkuVariantColorDto(string Color, Guid ProductSkuId, bool InStock);

internal static class ProductSkuWithVariantsDtoMapper
{
    public static ProductSkuWithVariantsDto ToDto(this ProductSkuWithVariants model) =>
        new(
            model.ProductSku.ToDto(),
            model
                .Variants.Select(variant => new ProductSkuVariantDto(
                    variant.Size.Value,
                    variant.ProductSkuId.Value,
                    variant.InStock,
                    variant
                        .Colors.Select(color => new ProductSkuVariantColorDto(
                            color.Color.Value,
                            color.ProductSkuId.Value,
                            color.InStock
                        ))
                        .ToList()
                ))
                .ToList()
        );
}
