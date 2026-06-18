using Domain.Abstractions;

namespace Domain.Shared.ValueObjects;

public sealed record Money
{
    public long Cents { get; }
    public decimal Dollars => Cents / 100m;

    private Money(long cents)
    {
        Cents = cents;
    }

    public static Result<Money> FromCents(long cents)
    {
        if (cents < 0)
        {
            return Error.Validation("money", ["Money amount cannot be negative."]);
        }

        return new Money(cents);
    }

    public static Result<Money> FromDollars(decimal dollars)
    {
        if (dollars < 0)
        {
            return Error.Validation("money", ["Money amount cannot be negative."]);
        }

        return new Money((long)Math.Round(dollars * 100m, MidpointRounding.AwayFromZero));
    }

    public static Money operator +(Money a, Money b) => FromCents(a.Cents + b.Cents).Value;

    public static Money operator -(Money a, Money b)
    {
        var result = a.Cents - b.Cents;
        return result < 0
            ? throw new InvalidOperationException("Money cannot be negative.")
            : new Money(result);
    }

    public static bool operator >(Money a, Money b) => a.Cents > b.Cents;

    public static bool operator <(Money a, Money b) => a.Cents < b.Cents;

    public static bool operator >=(Money a, Money b) => a.Cents >= b.Cents;

    public static bool operator <=(Money a, Money b) => a.Cents <= b.Cents;
}
