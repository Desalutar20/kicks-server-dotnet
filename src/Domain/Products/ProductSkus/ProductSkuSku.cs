using System.Text.RegularExpressions;
using Domain.Abstractions;
using Domain.Shared.ValueObjects;

namespace Domain.Products.ProductSkus;

public sealed record ProductSkuSku : StringValueObject<ProductSkuSku>
{
    public const int MaxLength = 30;

    private static readonly Regex Regex = new(
        @"\s+",
        RegexOptions.Compiled | RegexOptions.IgnoreCase,
        TimeSpan.FromMilliseconds(100)
    );

    private ProductSkuSku(string value)
        : base(value) { }

    public static Result<ProductSkuSku> Create(string value) =>
        CreateCore(
            value,
            MaxLength,
            "sku",
            "Product sku",
            (v) => new ProductSkuSku(v),
            static (v) => Regex.Replace(v, "")
        );

    public override string ToString() => Value;
}
