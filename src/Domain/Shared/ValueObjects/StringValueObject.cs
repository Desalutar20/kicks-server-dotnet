using Domain.Abstractions;

namespace Domain.Shared.ValueObjects;

public abstract record StringValueObject<T>(string Value)
    where T : notnull
{
    protected static Result<T> CreateCore(
        string value,
        int maxLength,
        string validationField,
        string label,
        Func<string, T> factory,
        Func<string, string>? transform = null,
        int minLength = 1
    )
    {
        var errors = new List<string>();

        var emptyResult = Guard.AgainstEmptyString(value, label);
        if (emptyResult.IsFailure)
        {
            errors.Add(emptyResult.Error.Description);
        }

        value = value.Trim();

        if (transform is not null)
        {
            value = transform(value);
        }

        var lengthResult = Guard.ForStringLength(
            value,
            minLength <= 0 ? 1 : minLength,
            maxLength,
            label
        );
        if (lengthResult.IsFailure)
        {
            errors.Add(lengthResult.Error.Description);
        }

        return errors.Count == 0 ? factory(value) : Error.Validation(validationField, errors);
    }

    public override string ToString() => Value;
};
