using Domain.Abstractions;
using Domain.Shared;

namespace Domain.User;

public sealed record LastName
{
    public const int MaxLength = 30;

    private LastName(string value)
    {
        Value = value;
    }

    public string Value { get; }

    public static Result<LastName> Create(string value)
    {
        var errors = new List<string>();

        value = value.Trim();

        var emptyResult = Guard.AgainstEmptyString(value);
        if (emptyResult.IsFailure)
        {
            errors.Add(emptyResult.Error.Description);
        }

        var lengthResult = Guard.ForStringLength(value, 1, MaxLength, "Last name");
        if (lengthResult.IsFailure)
        {
            errors.Add(lengthResult.Error.Description);
        }

        return errors.Count == 0
            ? Result<LastName>.Success(new LastName(value))
            : Result<LastName>.Failure(Error.Validation("lastName", errors));
    }

    public override string ToString() => Value;

    public static implicit operator string(LastName firstName) => firstName.Value;

    public static implicit operator LastName(string value) => Create(value).Value;
}
