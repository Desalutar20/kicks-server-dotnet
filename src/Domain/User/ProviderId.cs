using Domain.Abstractions;
using Domain.Shared;

namespace Domain.User;

public sealed record ProviderId
{
    public const int MaxLength = 100;

    private ProviderId(string value)
    {
        Value = value;
    }

    public string Value { get; }

    public static Result<ProviderId> Create(string value)
    {
        var errors = new List<string>();

        value = value.Trim();

        var emptyResult = Guard.AgainstEmptyString(value);
        if (emptyResult.IsFailure)
        {
            errors.Add(emptyResult.Error.Description);
        }

        var lengthResult = Guard.ForStringLength(value, 1, MaxLength, "Provider id");
        if (lengthResult.IsFailure)
        {
            errors.Add(lengthResult.Error.Description);
        }

        return errors.Count == 0
            ? Result<ProviderId>.Success(new ProviderId(value))
            : Result<ProviderId>.Failure(Error.Validation("providerId", errors));
    }

    public override string ToString() => Value;
}
