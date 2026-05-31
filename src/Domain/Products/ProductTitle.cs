using Domain.Abstractions;
using Domain.Shared.ValueObjects;

namespace Domain.Products;

public sealed record ProductTitle : StringValueObject<ProductTitle>
{
    public const int MaxLength = 60;

    private ProductTitle(string value)
        : base(value) { }

    public static Result<ProductTitle> Create(string value) =>
        CreateCore(value, MaxLength, "title", "Product title", (val) => new ProductTitle(val));

    public static implicit operator string(ProductTitle title) => title.Value;
}
