using Domain.Abstractions;
using Domain.Shared;

namespace Domain.Products.ProductSkus.ProductSkuReviews;

public sealed record ProductSkuReviewDescription
{
    public const int MaxLength = 400;

    private ProductSkuReviewDescription(string value)
    {
        Value = value;
    }

    public string Value { get; }

    public static Result<ProductSkuReviewDescription> Create(string value)
    {
        var errors = new List<string>();

        var emptyResult = Guard.AgainstEmptyString(value);
        if (emptyResult.IsFailure)
        {
            errors.Add(emptyResult.Error.Description);
        }

        value = value.Trim();

        var lengthResult = Guard.ForStringLength(
            value,
            1,
            MaxLength,
            "Product sku review description"
        );
        if (lengthResult.IsFailure)
        {
            errors.Add(lengthResult.Error.Description);
        }

        return errors.Count == 0
            ? new ProductSkuReviewDescription(value)
            : Error.Validation("description", errors);
    }

    public override string ToString() => Value;

    public static implicit operator string(ProductSkuReviewDescription description) =>
        description.Value;

    public static implicit operator ProductSkuReviewDescription(string value) =>
        Create(value).Value;
}
