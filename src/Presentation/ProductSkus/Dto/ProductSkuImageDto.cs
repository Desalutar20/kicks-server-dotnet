using Domain.Product.ProductSku;

namespace Presentation.ProductSkus.Dto;

public record ProductSkuImageDto(Guid ImageId, string ImageUrl, string ImageName);

internal static class ProductSkuImageDtoMapper
{
    public static ProductSkuImageDto ToDto(this ProductSkuImage model) =>
        new(model.ImageId, model.ImageUrl.Value, model.ImageName.Value);
}
