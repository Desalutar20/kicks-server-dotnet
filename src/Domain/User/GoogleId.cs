using Domain.Abstractions;
using Domain.Shared;

namespace Domain.User;

public readonly record struct GoogleId
{
    public const int MaxLength = 100;

    private GoogleId(string value)
    {
        Value = value;
    }

    public string Value { get; }

    public static Result<GoogleId> Create(string value)
    {
        var errors = new List<string>();

        value = value.Trim();

        var emptyResult = Guard.AgainstEmptyString(value);
        if (emptyResult.IsFailure) errors.Add(emptyResult.Error.Description);

        var lengthResult = Guard.ForStringLength(value, 1, MaxLength, "Google id");
        if (lengthResult.IsFailure) errors.Add(lengthResult.Error.Description);


        return errors.Count == 0
            ? Result<GoogleId>.Success(new GoogleId(value))
            : Result<GoogleId>.Failure(Error.Validation("googleId", errors));
    }

    public override string ToString() => Value;
}