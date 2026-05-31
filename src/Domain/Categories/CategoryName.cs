using Domain.Abstractions;
using Domain.Shared;
using Domain.Shared.ValueObjects;

namespace Domain.Categories;

public sealed record CategoryName : StringValueObject<CategoryName>
{
    public const int MaxLength = 30;

    private CategoryName(string value)
        : base(value) { }

    public static Result<CategoryName> Create(string value) =>
        CreateCore(value, MaxLength, "category", "Category name", (val) => new CategoryName(val));

    public static implicit operator string(CategoryName category) => category.Value;
}
