using Domain.Abstractions;

namespace Domain.Products;

public sealed record ProductTags
{
    public const int MaxTags = 20;

    private ProductTags(List<string> value)
    {
        Value = value;
    }

    public List<string> Value { get; } = [];

    public static Result<ProductTags> Create(List<string> value)
    {
        return value.Count > MaxTags
            ? Error.Validation("tags", [$"Product cannot have more than {MaxTags} tags."])
            : new ProductTags([.. value.Where(x => !string.IsNullOrWhiteSpace(x)).Distinct()]);
    }

    public static ProductTags Empty() => new([]);
}
