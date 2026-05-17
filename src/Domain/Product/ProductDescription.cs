using Domain.Abstractions;
using Domain.Shared;

namespace Domain.Product;

public sealed record ProductDescription
{
    public const int MaxLength = 200;

    private ProductDescription(string value)
    {
        Value = value;
    }

    public string Value { get; }

    public static Result<ProductDescription> Create(string value)
    {
        var errors = new List<string>();

        value = value.Trim();

        var emptyResult = Guard.AgainstEmptyString(value);
        if (emptyResult.IsFailure)
        {
            errors.Add(emptyResult.Error.Description);
        }

        var lengthResult = Guard.ForStringLength(value, 1, MaxLength, "Product description");
        if (lengthResult.IsFailure)
        {
            errors.Add(lengthResult.Error.Description);
        }

        return errors.Count == 0
            ? Result<ProductDescription>.Success(new ProductDescription(value))
            : Result<ProductDescription>.Failure(Error.Validation("productDescription", errors));
    }

    public override string ToString() => Value;

    public static implicit operator string(ProductDescription description) => description.Value;

    public static implicit operator ProductDescription(string value) => Create(value).Value;
};
