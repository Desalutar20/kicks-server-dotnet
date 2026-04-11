using Domain.Abstractions;
using Domain.Shared;

namespace Domain.User;

public readonly record struct FirstName
{
    public const int MaxLength = 30;

    private FirstName(string value)
    {
        Value = value;
    }

    public string Value { get; }

    public static Result<FirstName> Create(string value)
    {
        var errors = new List<string>();

        value = value.Trim();

        var emptyResult = Guard.AgainstEmptyString(value);
        if (emptyResult.IsFailure) errors.Add(emptyResult.Error.Description);

        var lengthResult = Guard.ForStringLength(value, 1, MaxLength, "First name");
        if (lengthResult.IsFailure) errors.Add(lengthResult.Error.Description);


        return errors.Count == 0
            ? Result<FirstName>.Success(new FirstName(value))
            : Result<FirstName>.Failure(Error.Validation("firstName", errors));
    }

    public override string ToString() => Value;
}