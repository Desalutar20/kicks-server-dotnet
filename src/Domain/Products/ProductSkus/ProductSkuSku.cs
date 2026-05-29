using System.Text.RegularExpressions;
using Domain.Abstractions;
using Domain.Shared;

namespace Domain.Products.ProductSkus;

public sealed record ProductSkuSku
{
    public const int MaxLength = 30;

    private static readonly Regex Regex = new(
        @"\s+",
        RegexOptions.Compiled | RegexOptions.IgnoreCase,
        TimeSpan.FromMilliseconds(100)
    );

    private ProductSkuSku(string value)
    {
        Value = value;
    }

    public string Value { get; }

    public static Result<ProductSkuSku> Create(string value)
    {
        var errors = new List<string>();

        var emptyResult = Guard.AgainstEmptyString(value, "Product sku");
        if (emptyResult.IsFailure)
        {
            errors.Add(emptyResult.Error.Description);
        }

        value = Regex.Replace(value, "");

        var lengthResult = Guard.ForStringLength(value, 1, MaxLength, "Product sku");
        if (lengthResult.IsFailure)
        {
            errors.Add(lengthResult.Error.Description);
        }

        return errors.Count == 0 ? new ProductSkuSku(value) : Error.Validation("sku", errors);
    }

    public override string ToString() => Value;

    public static implicit operator string(ProductSkuSku sku) => sku.Value;

    public static implicit operator ProductSkuSku(string value) => Create(value).Value;
}
