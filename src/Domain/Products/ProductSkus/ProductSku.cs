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
    public PositiveInt Quantity { get; private set; }
    public PositiveInt Size { get; private set; }
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
        ProductId productId,
        List<ProductSkuImage> images
    )
    {
        if (images is null || images.Count == 0 || images.Count > MaxImages)
        {
            return Error.Validation(
                "productSkuImages",
                [$"Product SKU must contain between 1 and {MaxImages} images."]
            );
        }

        var productSku = new ProductSku()
        {
            Price = price,
            Quantity = quantity,
            Color = color,
            Sku = sku,
            Size = size,
            ProductId = productId,
        };

        productSku._images.AddRange(images);

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

    public Result Update(
        ProductSkuPrice? price,
        PositiveInt? quantity,
        PositiveInt? size,
        ProductSkuColor? color,
        ProductSkuSku? sku
    )
    {
        if (price is not null)
        {
            var salePrice = price.SalePrice ?? Price.SalePrice;

            if (salePrice is not null && price.Price < salePrice)
            {
                return Error.Validation("price", ["Sale price cannot exceed price"]);
            }

            Price = ProductSkuPrice.Create(price.Price, salePrice).Value;
        }

        Quantity = quantity ?? Quantity;
        Size = size ?? Size;
        Color = color ?? Color;
        Sku = sku ?? Sku;

        return Result.Success();
    }
}
