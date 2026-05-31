using Domain.Abstractions;

namespace Domain.Shared;

public record NonEmptyString
{
    private NonEmptyString(string value)
    {
        Value = value;
    }

    public string Value { get; }

    public static Result<NonEmptyString> Create(
        string value,
        string field = "value",
        string label = "Value"
    )
    {
        if (Guard.AgainstEmptyString(value).IsFailure)
            return Error.Validation(field, [$"{label} cannot be empty."]);

        return new NonEmptyString(value.Trim());
    }

    public override string ToString() => Value;
}
