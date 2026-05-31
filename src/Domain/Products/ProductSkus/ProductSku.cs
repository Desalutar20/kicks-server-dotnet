using Domain.Abstractions;
using Domain.Products.ProductSkus.ProductSkuReviews;
using Domain.Shared;

namespace Domain.Products.ProductSkus;

public sealed class ProductSku : Entity<ProductSkuId>
{
    public const int MaxImages = 4;

    private ProductSku()
        : base(new ProductSkuId(Guid.NewGuid())) { }

    public ProductSkuPrice Price { get; private set; } = null!;
    public PositiveInt Quantity { get; private set; } = null!;
    public PositiveInt Size { get; private set; } = null!;
    public ProductSkuColor Color { get; private set; } = null!;
    public ProductSkuSku Sku { get; private set; } = null!;

    private readonly List<ProductSkuImage> _images = [];
    public IReadOnlyList<ProductSkuImage> Images => _images;

    public int RemainingImageSlots => MaxImages - _images.Count;
    public ProductId ProductId { get; private set; } = null!;
    public Product Product { get; private set; } = null!;

    private readonly List<ProductSkuReview> _reviews = [];
    public IReadOnlyList<ProductSkuReview> Reviews => _reviews;

    public static Result<ProductSku> Create(
        ProductSkuPrice price,
        PositiveInt quantity,
        ProductSkuColor color,
        ProductSkuSku sku,
        PositiveInt size,
        ProductId productId
    )
    {
        var productSku = new ProductSku()
        {
            Price = price,
            Quantity = quantity,
            Color = color,
            Sku = sku,
            Size = size,
            ProductId = productId,
        };

        return productSku;
    }

    public Result AddImages(List<ProductSkuImage> images)
    {
        if (images is null || images.Count == 0 || images.Count > RemainingImageSlots)
        {
            return Error.Validation(
                "productSkuImages",
                [$"Cannot add more than {MaxImages} images."]
            );
        }

        _images.AddRange(images);

        return Result.Success();
    }

    public Result<Guid> RemoveImage(Guid imageId)
    {
        if (_images.Count == 1)
        {
            return Error.Failure("Product SKU must have at least one image.");
        }

        var image = _images.FirstOrDefault(image => image.Id == imageId);
        if (image == null)
        {
            return Error.Failure("Product SKU image was not found.");
        }

        _images.RemoveAll(x => x.Id == imageId);

        return image.Id;
    }

    public void Update(
        ProductSkuPrice price,
        PositiveInt quantity,
        PositiveInt size,
        ProductSkuColor color,
        ProductSkuSku sku
    )
    {
        Price = price;
        Quantity = quantity;
        Size = size;
        Color = color;
        Sku = sku;
    }
}
