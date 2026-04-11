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
        var result = Guard.AgainstOutOfRange(value, 0, int.MaxValue);

        if (result.IsFailure)
            return Result<PositiveInt>.Failure(
                Error.Validation(field, ["Value must be a positive integer."]));

        return Result<PositiveInt>.Success(new PositiveInt(value));
    }
}