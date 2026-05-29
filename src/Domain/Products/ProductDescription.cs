using Domain.Abstractions;
using Domain.Shared;

namespace Domain.Products;

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

        var emptyResult = Guard.AgainstEmptyString(value, "Product description");
        if (emptyResult.IsFailure)
        {
            errors.Add(emptyResult.Error.Description);
        }

        value = value.Trim();

        var lengthResult = Guard.ForStringLength(value, 1, MaxLength, "Product description");
        if (lengthResult.IsFailure)
        {
            errors.Add(lengthResult.Error.Description);
        }

        return errors.Count == 0
            ? new ProductDescription(value)
            : Error.Validation("description", errors);
    }

    public override string ToString() => Value;

    public static implicit operator string(ProductDescription description) => description.Value;

    public static implicit operator ProductDescription(string value) => Create(value).Value;
}
