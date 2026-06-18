using Domain.Abstractions;
using Domain.Shared.ValueObjects;

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

    public bool IsValid =>
        DateTimeOffset.UtcNow >= ValidityPeriod.ValidFrom
        && DateTimeOffset.UtcNow < ValidityPeriod.ValidTo
        && UsageCount < UsageLimit.Value;

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

    public Money CalculateDiscount(Money subtotal)
    {
        if (!IsValid)
            return Money.FromCents(0).Value;

        var discountCents = Type switch
        {
            PromocodeType.Fixed => DiscountValue.Value * 100,
            PromocodeType.Percent => subtotal.Cents * DiscountValue.Value / 100,

            _ => 0,
        };

        if (discountCents <= 0)
            return Money.FromCents(0).Value;

        if (discountCents > subtotal.Cents)
            discountCents = subtotal.Cents;

        return Money.FromCents(discountCents).Value;
    }

    public Result IncreaseUsageCount()
    {
        if (++UsageCount > UsageLimit.Value)
        {
            return Error.Failure("The coupon usage limit has been reached.");
        }

        return Result.Success();
    }

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
