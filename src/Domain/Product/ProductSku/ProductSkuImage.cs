namespace Domain.Product.ProductSku;

public sealed record ProductSkuImage
{
    public ProductSkuImageUrl ImageUrl { get; private set; } = null!;
    public Guid ImageId { get; private set; }
    public ProductSkuImageName ImageName { get; private set; } = null!;

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
        };
}
