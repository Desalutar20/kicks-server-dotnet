using Domain.Abstractions;
using Domain.Shared;

namespace Domain.Product.ProductSku.ProductSkuImage;

public sealed record ProductSkuImageName
{
    public const int MaxLength = 100;

    private ProductSkuImageName(string value)
    {
        Value = value;
    }

    public string Value { get; }

    public static Result<ProductSkuImageName> Create(string value)
    {
        var errors = new List<string>();

        value = value.Trim();

        var emptyResult = Guard.AgainstEmptyString(value);
        if (emptyResult.IsFailure)
        {
            errors.Add(emptyResult.Error.Description);
        }

        var lengthResult = Guard.ForStringLength(value, 1, MaxLength, "Product sku image name");
        if (lengthResult.IsFailure)
        {
            errors.Add(lengthResult.Error.Description);
        }

        return errors.Count == 0
            ? Result<ProductSkuImageName>.Success(new ProductSkuImageName(value))
            : Result<ProductSkuImageName>.Failure(Error.Validation("productSkuImageName", errors));
    }

    public override string ToString() => Value;

    public static implicit operator string(ProductSkuImageName brand) => brand.Value;

    public static implicit operator ProductSkuImageName(string value) => Create(value).Value;
};
