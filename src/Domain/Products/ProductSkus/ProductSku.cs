using Domain.Abstractions;
using Domain.Shared.ValueObjects;

namespace Domain.Products.ProductSkus;

public sealed class ProductSku : Entity<ProductSkuId>
{
    public const int MaxImages = 4;

    private ProductSku()
        : base(new ProductSkuId(Guid.NewGuid())) { }

    public ProductSkuPrice Price { get; private set; } = null!;
    public int Quantity { get; private set; }
    public PositiveInt Size { get; private set; } = null!;
    public ProductSkuColor Color { get; private set; } = null!;
    public ProductSkuSku Sku { get; private set; } = null!;

    private readonly List<ProductSkuImage> _images = [];
    public IReadOnlyList<ProductSkuImage> Images => _images;

    public int RemainingImageSlots => MaxImages - _images.Count;
    public ProductId ProductId { get; private set; } = null!;
    public Product Product { get; private set; } = null!;

    public static Result<ProductSku> Create(
        ProductSkuPrice price,
        PositiveInt quantity,
        ProductSkuColor color,
        ProductSkuSku sku,
        PositiveInt size,
        List<ProductSkuImage> images,
        ProductId productId
    )
    {
        var productSku = new ProductSku()
        {
            Price = price,
            Quantity = quantity.Value,
            Color = color,
            Sku = sku,
            Size = size,
            ProductId = productId,
        };

        if (images is null || images.Count == 0 || images.Count > MaxImages)
        {
            return Error.Validation(
                "productSkuImages",
                [$"Product SKU must contain 1 to {MaxImages} images."]
            );
        }

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
        ProductSkuPrice price,
        int quantity,
        PositiveInt size,
        ProductSkuColor color,
        ProductSkuSku sku
    )
    {
        if (quantity < 0)
        {
            return Error.Failure("Quantity can not be negative");
        }

        Price = price;
        Quantity = quantity;
        Size = size;
        Color = color;
        Sku = sku;

        return Result.Success();
    }

    public Result DecreaseQuantity(PositiveInt amount)
    {
        if (Quantity < amount.Value)
            return Error.Failure("Not enough stock.");

        Quantity -= amount.Value;

        return Result.Success();
    }
}
