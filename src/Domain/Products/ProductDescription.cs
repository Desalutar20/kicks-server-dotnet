using Domain.Abstractions;
using Domain.Shared.ValueObjects;

namespace Domain.Products;

public sealed record ProductDescription : StringValueObject<ProductDescription>
{
    public const int MaxLength = 200;

    private ProductDescription(string value)
        : base(value) { }

    public static Result<ProductDescription> Create(string value) =>
        CreateCore(
            value,
            MaxLength,
            "description",
            "Product description",
            (v) => new ProductDescription(v)
        );

    public static implicit operator string(ProductDescription description) => description.Value;

    // public static implicit operator ProductDescription(string value) => Create(value).Value;
}
