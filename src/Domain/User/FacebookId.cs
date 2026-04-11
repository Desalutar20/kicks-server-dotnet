using Domain.Abstractions;
using Domain.Shared;

namespace Domain.User;

public readonly record struct FacebookId
{
    public const int MaxLength = 100;

    private FacebookId(string value)
    {
        Value = value;
    }

    public string Value { get; }

    public static Result<FacebookId> Create(string value)
    {
        var errors = new List<string>();

        value = value.Trim();

        var emptyResult = Guard.AgainstEmptyString(value);
        if (emptyResult.IsFailure) errors.Add(emptyResult.Error.Description);

        var lengthResult = Guard.ForStringLength(value, 1, MaxLength, "Facebook id");
        if (lengthResult.IsFailure) errors.Add(lengthResult.Error.Description);


        return errors.Count == 0
            ? Result<FacebookId>.Success(new FacebookId(value))
            : Result<FacebookId>.Failure(Error.Validation("facebookId", errors));
    }

    public override string ToString() => Value;
}