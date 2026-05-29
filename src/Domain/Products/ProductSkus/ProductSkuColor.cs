using System.Text.RegularExpressions;
using Domain.Abstractions;
using Domain.Shared;

namespace Domain.Products.ProductSkus;

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

        var emptyResult = Guard.AgainstEmptyString(value, "Color");
        if (emptyResult.IsFailure)
        {
            errors.Add(emptyResult.Error.Description);
        }

        value = value.Trim();

        if (!HexPattern.IsMatch(value))
        {
            errors.Add("Color must be a valid HEX format (e.g. #FFFFFF).");
        }

        return errors.Count == 0 ? new ProductSkuColor(value) : Error.Validation("color", errors);
    }

    public override string ToString() => Value;
}
