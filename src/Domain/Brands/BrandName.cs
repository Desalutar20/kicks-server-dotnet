using Domain.Abstractions;
using Domain.Shared;

namespace Domain.Brands;

public sealed record BrandName
{
    public const int MaxLength = 30;

    private BrandName(string value)
    {
        Value = value;
    }

    public string Value { get; }

    public static Result<BrandName> Create(string value)
    {
        var errors = new List<string>();

        var emptyResult = Guard.AgainstEmptyString(value, "Brand name");
        if (emptyResult.IsFailure)
        {
            errors.Add(emptyResult.Error.Description);
        }

        value = value.Trim();

        var lengthResult = Guard.ForStringLength(value, 1, MaxLength, "Brand name");
        if (lengthResult.IsFailure)
        {
            errors.Add(lengthResult.Error.Description);
        }

        return errors.Count == 0 ? new BrandName(value) : Error.Validation("brand", errors);
    }

    public override string ToString() => Value;

    public static implicit operator string(BrandName brand) => brand.Value;

    public static implicit operator BrandName(string value) => Create(value).Value;
}
