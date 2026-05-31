using System.Text.RegularExpressions;
using Domain.Abstractions;
using Domain.Shared.ValueObjects;

namespace Domain.Promocodes;

public sealed record PromocodeCode : StringValueObject<PromocodeCode>
{
    public const int MaxLength = 30;

    private static readonly Regex Regex = new(
        @"\s+",
        RegexOptions.Compiled | RegexOptions.IgnoreCase,
        TimeSpan.FromMilliseconds(100)
    );

    private PromocodeCode(string value)
        : base(value) { }

    public static Result<PromocodeCode> Create(string value) =>
        CreateCore(
            value,
            MaxLength,
            "sku",
            "Product sku",
            (v) => new PromocodeCode(v),
            static (v) => Regex.Replace(v, "")
        );
}
