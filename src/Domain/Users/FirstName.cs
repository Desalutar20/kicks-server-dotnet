using Domain.Abstractions;
using Domain.Shared;

namespace Domain.Users;

public sealed record FirstName
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

        var emptyResult = Guard.AgainstEmptyString(value, "First name");
        if (emptyResult.IsFailure)
        {
            errors.Add(emptyResult.Error.Description);
        }

        value = value.Trim();

        var lengthResult = Guard.ForStringLength(value, 1, MaxLength, "First name");
        if (lengthResult.IsFailure)
        {
            errors.Add(lengthResult.Error.Description);
        }

        return errors.Count == 0 ? new FirstName(value) : Error.Validation("firstName", errors);
    }

    public override string ToString() => Value;

    public static implicit operator string(FirstName firstName) => firstName.Value;

    public static implicit operator FirstName(string value) => Create(value).Value;
}
