using Domain.Abstractions;
using Domain.Shared;

namespace Domain.Users;

public sealed record Password
{
    public const int MinLength = 8;
    public const int MaxLength = 40;

    private Password(string value)
    {
        Value = value;
    }

    public string Value { get; }

    public static Result<Password> Create(string value)
    {
        var errors = new List<string>();

        var emptyResult = Guard.AgainstEmptyString(value, "Password");
        if (emptyResult.IsFailure)
        {
            errors.Add(emptyResult.Error.Description);
        }

        value = value.Trim();

        var lengthResult = Guard.ForStringLength(value, MinLength, MaxLength, "Password");
        if (lengthResult.IsFailure)
        {
            errors.Add(lengthResult.Error.Description);
        }

        return errors.Count == 0 ? new Password(value) : Error.Validation("password", errors);
    }

    public override string ToString() => Value;
}
