using Domain.Abstractions;

namespace Domain.Promocodes;

public sealed record PromocodeValidityPeriod
{
    public DateTimeOffset ValidFrom { get; }
    public DateTimeOffset ValidTo { get; }

    private PromocodeValidityPeriod(DateTimeOffset validFrom, DateTimeOffset validTo)
    {
        ValidFrom = validFrom;
        ValidTo = validTo;
    }

    public static Result<PromocodeValidityPeriod> Create(
        DateTimeOffset validFrom,
        DateTimeOffset validTo
    )
    {
        if (DateTimeOffset.UtcNow > validTo)
        {
            return Error.Validation("validTo", ["Valid to date must be in the future."]);
        }

        if (validTo <= validFrom)
        {
            return Error.Validation(
                "validTo",
                ["Valid to date must be greater than valid from date."]
            );
        }

        return new PromocodeValidityPeriod(validFrom, validTo);
    }
};
