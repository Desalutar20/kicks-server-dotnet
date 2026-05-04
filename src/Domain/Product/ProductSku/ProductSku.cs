using Domain.Abstractions;
using Domain.Shared;

namespace Domain.Product.ProductSku;

public sealed class ProductSku : Entity<ProductSkuId>
{
    private const int MaxImages = 4;

    private ProductSku()
        : base(new ProductSkuId(Guid.NewGuid())) { }

    public ProductSkuPrice Price { get; private set; } = null!;
    public PositiveInt Quantity { get; private set; }
    public PositiveInt Size { get; private set; }
    public ProductSkuColor Color { get; private set; } = null!;

    public ProductId ProductId { get; private set; } = null!;
    public Product Product { get; private set; } = null!;
    public List<ProductSkuImage.ProductSkuImage> ProductSkuImages { get; private set; } = null!;

    public static Result<ProductSku> Create(
        ProductSkuPrice price,
        PositiveInt quantity,
        PositiveInt size,
        ProductSkuColor color,
        ProductId productId,
        List<ProductSkuImage.ProductSkuImage> productSkuImages
    )
    {
        if (productSkuImages is null || productSkuImages.Count == 0)
        {
            return Result<ProductSku>.Failure(
                Error.Validation(
                    "productSkuImages",
                    [$"Product SKU must contain between 1 and {MaxImages} images."]
                )
            );
        }

        if (productSkuImages.Count > MaxImages)
        {
            return Result<ProductSku>.Failure(
                Error.Validation(
                    "productSkuImages",
                    [$"Product SKU must contain between 1 and {MaxImages} images."]
                )
            );
        }

        return Result<ProductSku>.Success(
            new ProductSku
            {
                Price = price,
                Quantity = quantity,
                Size = size,
                Color = color,
                ProductId = productId,
                ProductSkuImages = productSkuImages,
            }
        );
    }
}
