using Domain.Abstractions;
using Domain.Shared;

namespace Domain.Product;

public sealed record ProductTitle
{
    public const int MaxLength = 50;

    private ProductTitle(string value)
    {
        Value = value;
    }

    public string Value { get; }

    public static Result<ProductTitle> Create(string value)
    {
        var errors = new List<string>();

        value = value.Trim();

        var emptyResult = Guard.AgainstEmptyString(value);
        if (emptyResult.IsFailure)
        {
            errors.Add(emptyResult.Error.Description);
        }

        var lengthResult = Guard.ForStringLength(value, 1, MaxLength, "Product title");
        if (lengthResult.IsFailure)
        {
            errors.Add(lengthResult.Error.Description);
        }

        return errors.Count == 0
            ? Result<ProductTitle>.Success(new ProductTitle(value))
            : Result<ProductTitle>.Failure(Error.Validation("productTitle", errors));
    }

    public override string ToString() => Value;

    public static implicit operator string(ProductTitle brand) => brand.Value;

    public static implicit operator ProductTitle(string value) => Create(value).Value;
};
