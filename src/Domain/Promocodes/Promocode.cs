using Domain.Abstractions;
using Domain.Shared;

namespace Domain.Promocodes;

public sealed class Promocode : Entity<PromocodeId>
{
    private Promocode()
        : base(new PromocodeId(Guid.NewGuid())) { }

    private Promocode(
        PositiveInt discountValue,
        PromocodeType type,
        PromocodeValidityPeriod validityPeriod,
        PositiveInt usageLimit,
        PromocodeCode code
    )
        : base(new PromocodeId(Guid.NewGuid()))
    {
        DiscountValue = discountValue;
        Type = type;
        ValidityPeriod = validityPeriod;
        UsageLimit = usageLimit;
        Code = code;
    }

    public PositiveInt DiscountValue { get; private set; } = null!;
    public PromocodeType Type { get; private set; }
    public PromocodeValidityPeriod ValidityPeriod { get; private set; } = null!;
    public PositiveInt UsageLimit { get; private set; } = null!;
    public int UsageCount { get; private set; } = 0;
    public PromocodeCode Code { get; private set; } = null!;

    public bool IsActive =>
        DateTimeOffset.UtcNow >= ValidityPeriod.ValidFrom
        && DateTimeOffset.UtcNow < ValidityPeriod.ValidTo;

    public bool IsExpired => DateTimeOffset.UtcNow >= ValidityPeriod.ValidTo;

    public bool HasUsagesLeft => UsageCount < UsageLimit.Value;

    public static Result<Promocode> Create(
        PositiveInt discountValue,
        PromocodeType type,
        PromocodeValidityPeriod validityPeriod,
        PositiveInt usageLimit,
        PromocodeCode code
    )
    {
        var result = ValidateDiscount(type, discountValue);
        if (result.IsFailure)
        {
            return result.Error;
        }

        return new Promocode(discountValue, type, validityPeriod, usageLimit, code);
    }

    public Result Update(
        PositiveInt discountValue,
        PromocodeType type,
        PromocodeValidityPeriod validityPeriod,
        PositiveInt usageLimit,
        PromocodeCode code
    )
    {
        var result = ValidateDiscount(type, discountValue);
        if (result.IsFailure)
        {
            return result.Error;
        }

        DiscountValue = discountValue;
        Type = type;
        ValidityPeriod = validityPeriod;
        UsageLimit = usageLimit;
        Code = code;

        return Result.Success();
    }

    private static Result ValidateDiscount(PromocodeType type, PositiveInt discountValue)
    {
        if (
            type == PromocodeType.Percent
            && (discountValue.Value <= 0 || discountValue.Value >= 100)
        )
        {
            return Error.Validation(
                "discountValue",
                ["Percent discount must be greater than 0 and less than 100."]
            );
        }

        return Result.Success();
    }
}
