using Domain.Abstractions;
using Domain.Shared;

namespace Domain.Categories;

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

        var emptyResult = Guard.AgainstEmptyString(value, "Category name");
        if (emptyResult.IsFailure)
        {
            errors.Add(emptyResult.Error.Description);
        }

        value = value.Trim();

        var lengthResult = Guard.ForStringLength(value, 1, MaxLength, "Category name");
        if (lengthResult.IsFailure)
        {
            errors.Add(lengthResult.Error.Description);
        }

        return errors.Count == 0 ? new CategoryName(value) : Error.Validation("category", errors);
    }

    public override string ToString() => Value;

    public static implicit operator string(CategoryName category) => category.Value;

    public static implicit operator CategoryName(string value) => Create(value).Value;
}
