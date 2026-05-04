using Domain.Abstractions;

namespace Domain.Product.ProductSku.ProductSkuImage;

public sealed class ProductSkuImage : Entity<ProductSkuImageId>
{
    private ProductSkuImage()
        : base(new ProductSkuImageId(Guid.NewGuid())) { }

    public ProductSkuImageUrl ImageUrl { get; private set; } = null!;
    public Guid ImageId { get; private set; }
    public ProductSkuImageName ImageName { get; private set; } = null!;
    public ProductSkuId ProductSkuId { get; private set; } = null!;

    public static ProductSkuImage Create(
        ProductSkuImageUrl imageUrl,
        Guid imageId,
        ProductSkuImageName imageName,
        ProductSkuId productSkuId
    ) =>
        new()
        {
            ImageUrl = imageUrl,
            ImageId = imageId,
            ImageName = imageName,
            ProductSkuId = productSkuId,
        };
}
