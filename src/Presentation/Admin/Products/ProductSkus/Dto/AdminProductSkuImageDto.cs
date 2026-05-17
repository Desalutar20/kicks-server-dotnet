using Domain.Product.ProductSku.ProductSkuImage;

namespace Presentation.Admin.Products.ProductSkus.Dto;

public record AdminProductSkuImageDto(
    Guid Id,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt,
    Guid ImageId,
    string ImageUrl,
    string ImageName
);

internal static class AdminProductSkuImageDtoMapper
{
    public static AdminProductSkuImageDto ToDto(this ProductSkuImage model) =>
        new(
            model.Id.Value,
            model.CreatedAt,
            model.UpdatedAt,
            model.ImageId,
            model.ImageUrl.Value,
            model.ImageName.Value
        );
}
