using Domain.Abstractions;

namespace Domain.Shared;

public readonly record struct NonEmptyString
{
    private NonEmptyString(string value)
    {
        Value = value;
    }

    public string Value { get; }

    public static Result<NonEmptyString> Create(string value, string? field = "value")
    {
        value = value.Trim();

        if (Guard.AgainstEmptyString(value).IsFailure)
            return Result<NonEmptyString>.Failure(
                Error.Validation(value, ["Value cannot be empty."])
            );

        return Result<NonEmptyString>.Success(new NonEmptyString(value));
    }

    public override string ToString() => Value;
}
