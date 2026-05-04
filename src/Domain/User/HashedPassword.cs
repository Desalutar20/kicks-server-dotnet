using Domain.Abstractions;
using Domain.Shared;

namespace Domain.User;

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
        value = value.Trim();

        var emptyResult = Guard.AgainstEmptyString(value);
        if (emptyResult.IsFailure)
        {
            return Result<HashedPassword>.Failure(Error.Internal(emptyResult.Error.Description));
        }

        var lengthResult = Guard.ForStringLength(value, MinLength, MaxLength, "Hashed Password");
        if (lengthResult.IsFailure)
        {
            return Result<HashedPassword>.Failure(Error.Internal(lengthResult.Error.Description));
        }

        return Result<HashedPassword>.Success(new HashedPassword(value));
    }

    public override string ToString() => Value;
}
