using Domain.Abstractions;
using Domain.Shared;

namespace Domain.Category;

public sealed record CategoryName
{
    public const int MaxLength = 30;

    private CategoryName(string value)
    {
        Value = value;
    }

    public string Value { get; }

    public static Result<CategoryName> Create(string value)
    {
        var errors = new List<string>();

        value = value.Trim();

        var emptyResult = Guard.AgainstEmptyString(value);
        if (emptyResult.IsFailure)
        {
            errors.Add(emptyResult.Error.Description);
        }

        var lengthResult = Guard.ForStringLength(value, 1, MaxLength, "Category");
        if (lengthResult.IsFailure)
        {
            errors.Add(lengthResult.Error.Description);
        }

        return errors.Count == 0
            ? Result<CategoryName>.Success(new CategoryName(value))
            : Result<CategoryName>.Failure(Error.Validation("category", errors));
    }

    public override string ToString() => Value;

    public static implicit operator string(CategoryName category) => category.Value;

    public static implicit operator CategoryName(string value) => Create(value).Value;
}
