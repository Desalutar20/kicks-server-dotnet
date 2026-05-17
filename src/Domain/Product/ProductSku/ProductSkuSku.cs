using Domain.Abstractions;
using Domain.Shared;

namespace Domain.Product.ProductSku;

public sealed record ProductSkuSku
{
    public const int MaxLength = 30;

    private ProductSkuSku(string value)
    {
        Value = value;
    }

    public string Value { get; }

    public static Result<ProductSkuSku> Create(string value)
    {
        var errors = new List<string>();

        value = value.Trim();

        var emptyResult = Guard.AgainstEmptyString(value);
        if (emptyResult.IsFailure)
        {
            errors.Add(emptyResult.Error.Description);
        }

        var lengthResult = Guard.ForStringLength(value, 1, MaxLength, "Product sku sku");
        if (lengthResult.IsFailure)
        {
            errors.Add(lengthResult.Error.Description);
        }

        return errors.Count == 0
            ? Result<ProductSkuSku>.Success(new ProductSkuSku(value))
            : Result<ProductSkuSku>.Failure(Error.Validation("sku", errors));
    }

    public override string ToString() => Value;

    public static implicit operator string(ProductSkuSku sku) => sku.Value;

    public static implicit operator ProductSkuSku(string value) => Create(value).Value;
}
