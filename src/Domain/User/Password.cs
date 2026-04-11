using Domain.Abstractions;
using Domain.Shared;

namespace Domain.User;

public readonly record struct Password
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

        value = value.Trim();

        var emptyResult = Guard.AgainstEmptyString(value);
        if (emptyResult.IsFailure) errors.Add(emptyResult.Error.Description);

        var lengthResult = Guard.ForStringLength(value, MinLength, MaxLength, "Password");
        if (lengthResult.IsFailure) errors.Add(lengthResult.Error.Description);


        return errors.Count == 0
            ? Result<Password>.Success(new Password(value))
            : Result<Password>.Failure(Error.Validation("password", errors));
    }

    public override string ToString() => Value;
}