using Domain.Abstractions;
using Domain.Shared.ValueObjects;

namespace Domain.Brands;

public sealed record BrandName : StringValueObject<BrandName>
{
    public const int MaxLength = 30;

    private BrandName(string value)
        : base(value) { }

    public static Result<BrandName> Create(string value) =>
        CreateCore(value, MaxLength, "brand", "Brand name", (val) => new BrandName(val));

    public static implicit operator string(BrandName brand) => brand.Value;
}
