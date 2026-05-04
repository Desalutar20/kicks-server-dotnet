using Domain.Abstractions;
using Domain.Shared;

namespace Domain.Product.ProductSku.ProductSkuImage;

public sealed record ProductSkuImageUrl
{
    public const int MaxLength = 30;

    private ProductSkuImageUrl(string value)
    {
        Value = value;
    }

    public string Value { get; }

    public static Result<ProductSkuImageUrl> Create(string value)
    {
        var errors = new List<string>();

        value = value.Trim();

        var emptyResult = Guard.AgainstEmptyString(value, "Product sku image url");
        if (emptyResult.IsFailure)
        {
            errors.Add(emptyResult.Error.Description);
        }

        var lengthResult = Guard.ForStringLength(value, 1, MaxLength, "Product sku image url");
        if (lengthResult.IsFailure)
        {
            errors.Add(lengthResult.Error.Description);
        }

        if (
            !Uri.TryCreate(value, UriKind.Absolute, out var uri)
            || (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps)
        )
        {
            errors.Add("Image URL must be a valid HTTP or HTTPS URL.");
        }

        return errors.Count == 0
            ? Result<ProductSkuImageUrl>.Success(new ProductSkuImageUrl(value))
            : Result<ProductSkuImageUrl>.Failure(Error.Validation("productSkuImageUrl", errors));
    }
};
