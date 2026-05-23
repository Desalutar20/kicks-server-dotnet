using Domain.Product.ProductSku;

namespace Presentation.Admin.Products.ProductSkus.Dto;

public record AdminProductSkuImageDto(Guid ImageId, string ImageUrl, string ImageName);

internal static class AdminProductSkuImageDtoMapper
{
    public static AdminProductSkuImageDto ToDto(this ProductSkuImage model) =>
        new(model.ImageId, model.ImageUrl.Value, model.ImageName.Value);
}
