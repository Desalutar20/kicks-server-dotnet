using Domain.Abstractions;

namespace Domain.Product;

public sealed record ProductTags
{
    public const int MaxTags = 20;
    public List<string> Value { get; } = [];

    private ProductTags(List<string> value)
    {
        Value = value;
    }

    public static Result<ProductTags> Create(List<string> value)
    {
        return value.Count > MaxTags
            ? Result<ProductTags>.Failure(
                Error.Validation("productTags", [$"Product cannot have more than {MaxTags} tags."])
            )
            : Result<ProductTags>.Success(
                new ProductTags([.. value.Where(x => !string.IsNullOrWhiteSpace(x)).Distinct()])
            );
    }

    public static ProductTags Empty() => new([]);
};
