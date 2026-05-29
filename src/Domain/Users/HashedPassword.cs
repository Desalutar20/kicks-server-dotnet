using Domain.Abstractions;
using Domain.Shared;

namespace Domain.Users;

public sealed record HashedPassword
{
    public const int MinLength = 40;
    public const int MaxLength = 100;

    private HashedPassword(string value)
    {
        Value = value;
    }

    public string Value { get; }

    public static Result<HashedPassword> Create(string value)
    {
        var emptyResult = Guard.AgainstEmptyString(value);
        if (emptyResult.IsFailure)
        {
            return Error.Internal(emptyResult.Error.Description);
        }

        value = value.Trim();

        var lengthResult = Guard.ForStringLength(value, MinLength, MaxLength, "Hashed Password");
        if (lengthResult.IsFailure)
        {
            return Error.Internal(lengthResult.Error.Description);
        }

        return new HashedPassword(value);
    }

    public override string ToString() => Value;
}
