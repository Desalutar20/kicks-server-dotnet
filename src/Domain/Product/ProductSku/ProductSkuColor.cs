using System.Text.RegularExpressions;
using Domain.Abstractions;
using Domain.Shared;

namespace Domain.Product.ProductSku;

public sealed record ProductSkuColor
{
    private static readonly Regex HexPattern = new(
        "^#([0-9a-fA-F]{6})$",
        RegexOptions.Compiled | RegexOptions.IgnoreCase,
        TimeSpan.FromMilliseconds(100)
    );

    public string Value { get; }

    private ProductSkuColor(string value)
    {
        Value = value;
    }

    public static Result<ProductSkuColor> Create(string value)
    {
        var errors = new List<string>();

        value = value.Trim();

        var emptyResult = Guard.AgainstEmptyString(value, "Color");
        if (emptyResult.IsFailure)
        {
            errors.Add(emptyResult.Error.Description);
        }

        if (!HexPattern.IsMatch(value))
        {
            errors.Add("Color must be a valid HEX format (e.g. #FFFFFF).");
        }

        return errors.Count == 0
            ? Result<ProductSkuColor>.Success(new ProductSkuColor(value))
            : Result<ProductSkuColor>.Failure(Error.Validation("color", errors));
    }
}
