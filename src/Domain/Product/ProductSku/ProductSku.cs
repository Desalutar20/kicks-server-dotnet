using Domain.Abstractions;
using Domain.Shared;

namespace Domain.Product.ProductSku;

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

    public ProductId ProductId { get; private set; } = null!;
    public Product Product { get; private set; } = null!;

    public ProductSkuImages Images { get; private set; } = null!;

    public static ProductSku Create(
        ProductSkuPrice price,
        PositiveInt quantity,
        ProductSkuColor color,
        ProductSkuSku sku,
        PositiveInt size,
        ProductId productId
    ) =>
        new()
        {
            Price = price,
            Quantity = quantity,
            Color = color,
            Sku = sku,
            Size = size,
            ProductId = productId,
            Images = new ProductSkuImages([]),
        };

    public Result SetImages(List<ProductSkuImage> images)
    {
        if (images is null || images.Count == 0 || images.Count > MaxImages)
        {
            return Error.Validation(
                "productSkuImages",
                [$"Product SKU must contain between 1 and {MaxImages} images."]
            );
        }

        Images = new ProductSkuImages(images);

        return Result.Success();
    }

    public Result AddImages(List<ProductSkuImage> images)
    {
        if (images is null || images.Count == 0 || Images.Images.Count + images.Count > MaxImages)
        {
            return Error.Validation(
                "productSkuImages",
                [$"Cannot add more than {MaxImages} images."]
            );
        }

        Images.AddRange(images);

        return Result.Success();
    }

    public Result<Guid> RemoveImage(Guid imageId)
    {
        if (Images.Images.Count == 1)
        {
            return Error.Failure("Product SKU must have at least one image.");
        }

        var image = Images.Images.SingleOrDefault(image => image.ImageId == imageId);
        if (image == null)
        {
            return Error.Failure("Product SKU image was not found.");
        }

        Images.Remove(image.ImageId);

        return image.ImageId;
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
