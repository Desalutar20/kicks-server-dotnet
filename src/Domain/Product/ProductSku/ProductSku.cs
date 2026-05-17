using Domain.Abstractions;
using Domain.Product.ProductSku.ProductSkuImage;
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

    public List<ProductSkuImage.ProductSkuImage> ProductSkuImages { get; private set; } = [];

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
        };

    public Result SetImages(List<ProductSkuImage.ProductSkuImage> images)
    {
        if (images is null || images.Count == 0 || images.Count > MaxImages)
        {
            return Result.Failure(
                Error.Validation(
                    "productSkuImages",
                    [$"Product SKU must contain between 1 and {MaxImages} images."]
                )
            );
        }

        ProductSkuImages = images;

        return Result.Success();
    }

    public Result AddImages(List<ProductSkuImage.ProductSkuImage> images)
    {
        if (
            images is null
            || images.Count == 0
            || ProductSkuImages.Count + images.Count > MaxImages
        )
        {
            return Result.Failure(
                Error.Validation("productSkuImages", [$"Cannot add more than {MaxImages} images."])
            );
        }

        ProductSkuImages.AddRange(images);

        return Result.Success();
    }

    public Result<Guid> RemoveImage(ProductSkuImageId imageId)
    {
        if (ProductSkuImages.Count == 1)
        {
            return Result<Guid>.Failure(Error.Failure("Product SKU must have at least one image."));
        }

        var image = ProductSkuImages.SingleOrDefault(image => image.Id == imageId);
        if (image == null)
        {
            return Result<Guid>.Failure(Error.Failure("Product SKU image was not found."));
        }

        ProductSkuImages = ProductSkuImages.Where(image => image.Id != imageId).ToList();

        return Result<Guid>.Success(image.ImageId);
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
                return Result.Failure(
                    Error.Validation("price", ["Sale price cannot exceed price"])
                );
            }

            Price = ProductSkuPrice.Create(price.Price, salePrice).Value;
        }

        Quantity = quantity ?? Quantity;
        Size = size ?? Size;
        Color = color ?? Color;
        Sku = sku ?? Sku;

        return Result.Success();
    }

    public Result UpdatePrice(ProductSkuPrice newPrice)
    {
        var salePrice = newPrice.SalePrice ?? Price.SalePrice;

        if (salePrice is not null && newPrice.Price < salePrice)
        {
            return Result.Failure(Error.Validation("price", ["Sale price cannot exceed price"]));
        }

        Price = ProductSkuPrice.Create(newPrice.Price, salePrice).Value;

        return Result.Success();
    }
}
