using Domain.Abstractions;
using Domain.Shared;

namespace Domain.Users;

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

        var emptyResult = Guard.AgainstEmptyString(value, "Last name");
        if (emptyResult.IsFailure)
        {
            errors.Add(emptyResult.Error.Description);
        }

        value = value.Trim();

        var lengthResult = Guard.ForStringLength(value, 1, MaxLength, "Last name");
        if (lengthResult.IsFailure)
        {
            errors.Add(lengthResult.Error.Description);
        }

        return errors.Count == 0 ? new LastName(value) : Error.Validation("lastName", errors);
    }

    public override string ToString() => Value;

    public static implicit operator string(LastName firstName) => firstName.Value;

    public static implicit operator LastName(string value) => Create(value).Value;
}
