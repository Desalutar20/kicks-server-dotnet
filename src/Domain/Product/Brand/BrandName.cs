using Domain.Abstractions;
using Domain.Shared;

namespace Domain.Product.Brand;

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

        value = value.Trim();

        var emptyResult = Guard.AgainstEmptyString(value);
        if (emptyResult.IsFailure)
        {
            errors.Add(emptyResult.Error.Description);
        }

        var lengthResult = Guard.ForStringLength(value, 1, MaxLength, "Brand");
        if (lengthResult.IsFailure)
        {
            errors.Add(lengthResult.Error.Description);
        }

        return errors.Count == 0
            ? Result<BrandName>.Success(new BrandName(value))
            : Result<BrandName>.Failure(Error.Validation("brand", errors));
    }

    public override string ToString() => Value;

    public static implicit operator string(BrandName brand) => brand.Value;

    public static implicit operator BrandName(string value) => Create(value).Value;
}
