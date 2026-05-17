using Domain.Abstractions;

namespace Domain.Shared;

public readonly record struct PositiveInt
{
    private PositiveInt(int value)
    {
        Value = value;
    }

    public int Value { get; }

    public static Result<PositiveInt> Create(int value, string field = "value")
    {
        var result = Guard.AgainstOutOfRange(value, 1, int.MaxValue);

        if (result.IsFailure)
        {
            return Result<PositiveInt>.Failure(
                Error.Validation(field, ["Value must be a positive integer."])
            );
        }

        return Result<PositiveInt>.Success(new PositiveInt(value));
    }

    public static implicit operator int(PositiveInt positiveInt) => positiveInt.Value;

    public static PositiveInt operator +(PositiveInt left, PositiveInt right) =>
        new(left.Value + right.Value);

    public static PositiveInt operator -(PositiveInt left, PositiveInt right) =>
        new(left.Value - right.Value);

    public static PositiveInt operator *(PositiveInt left, PositiveInt right) =>
        new(left.Value * right.Value);

    public static PositiveInt operator /(PositiveInt left, PositiveInt right) =>
        new(left.Value / right.Value);

    public static bool operator >(PositiveInt left, PositiveInt right) => left.Value > right.Value;

    public static bool operator <(PositiveInt left, PositiveInt right) => left.Value < right.Value;

    public static bool operator >=(PositiveInt left, PositiveInt right) =>
        left.Value >= right.Value;

    public static bool operator <=(PositiveInt left, PositiveInt right) =>
        left.Value <= right.Value;

    public override string ToString() => Value.ToString();
}
