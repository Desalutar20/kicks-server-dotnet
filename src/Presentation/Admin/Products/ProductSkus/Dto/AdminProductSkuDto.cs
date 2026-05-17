using Domain.Product.ProductSku;
using Presentation.Admin.Products.Dto;

namespace Presentation.Admin.Products.ProductSkus.Dto;

public sealed record AdminProductSkuDto(
    Guid Id,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt,
    int Price,
    int? SalePrice,
    int Quantity,
    int Size,
    string Color,
    string Sku,
    AdminProductDto Product,
    List<AdminProductSkuImageDto> Images
);

internal static class AdminProductSkuDtoMapper
{
    public static AdminProductSkuDto ToDto(this ProductSku model) =>
        new(
            model.Id.Value,
            model.CreatedAt,
            model.UpdatedAt,
            model.Price.Price.Value,
            model.Price.SalePrice?.Value,
            model.Quantity.Value,
            model.Size.Value,
            model.Color.Value,
            model.Sku.Value,
            model.Product.ToDto(),
            [.. model.ProductSkuImages.Select(image => image.ToDto())]
        );
}
