using Domain.Product.ProductSku;

namespace Presentation.ProductSkus.Dto;

public sealed record ProductSkuDto(
    Guid Id,
    DateTimeOffset CreatedAt,
    int Price,
    int? SalePrice,
    int Quantity,
    int Size,
    string Color,
    string Sku,
    ProductDto Product,
    List<ProductSkuImageDto> Images
);

internal static class ProductSkuDtoMapper
{
    public static ProductSkuDto ToDto(this ProductSku model) =>
        new(
            model.Id.Value,
            model.CreatedAt,
            model.Price.Price.Value,
            model.Price.SalePrice?.Value,
            model.Quantity.Value,
            model.Size.Value,
            model.Color.Value,
            model.Sku.Value,
            model.Product.ToDto(),
            [.. model.Images.Images.Select(image => image.ToDto())]
        );
}
