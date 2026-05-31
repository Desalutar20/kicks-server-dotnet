using Domain.Abstractions;
using Domain.Shared.ValueObjects;

namespace Domain.Products.ProductSkus.ProductSkuReviews;

public sealed record ProductSkuReviewDescription : StringValueObject<ProductSkuReviewDescription>
{
    public const int MaxLength = 400;

    private ProductSkuReviewDescription(string value)
        : base(value) { }

    public static Result<ProductSkuReviewDescription> Create(string value) =>
        CreateCore(
            value,
            MaxLength,
            "description",
            "Product sku review description",
            (v) => new ProductSkuReviewDescription(v)
        );
}
