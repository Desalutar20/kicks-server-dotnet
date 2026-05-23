namespace Domain.Product.ProductSku;

public sealed class ProductSkuImages
{
    private readonly List<ProductSkuImage> _images = [];

    public IReadOnlyList<ProductSkuImage> Images => _images;

    private ProductSkuImages() { }

    public ProductSkuImages(List<ProductSkuImage> images)
    {
        _images = images;
    }

    public void AddRange(List<ProductSkuImage> images)
    {
        _images.AddRange(images);
    }

    public void Remove(Guid imageId)
    {
        _images.RemoveAll(x => x.ImageId == imageId);
    }
}
