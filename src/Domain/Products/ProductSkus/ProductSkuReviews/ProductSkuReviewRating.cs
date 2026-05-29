using Domain.Abstractions;
using Domain.Shared;

namespace Domain.Products.ProductSkus.ProductSkuReviews;

public sealed record ProductSkuReviewRating
{
    private ProductSkuReviewRating(int value)
    {
        Value = value;
    }

    public int Value { get; }

    public static Result<ProductSkuReviewRating> Create(int value, string field = "value")
    {
        var result = Guard.AgainstOutOfRange(value, 1, 5);

        if (result.IsFailure)
        {
            return Error.Validation(field, ["Rating must be between 1 and 5."]);
        }

        return new ProductSkuReviewRating(value);
    }

    public static implicit operator int(ProductSkuReviewRating positiveInt) => positiveInt.Value;
};
