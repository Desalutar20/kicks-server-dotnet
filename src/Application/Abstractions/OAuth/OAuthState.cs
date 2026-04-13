namespace Application.Abstractions.OAuth;

public sealed record OAuthState(Guid StateId, NonEmptyString? AdditionalState)
{
    private const string Delimiter = "|";

    public static Result<OAuthState> Create(string value)
    {
        var emptyResult = Guard.AgainstEmptyString(value);
        if (emptyResult.IsFailure)
        {
            return Result<OAuthState>.Failure(emptyResult.Error);
        }

        var parts = value.Split(Delimiter);

        if (!Guid.TryParse(parts[0], out var stateId))
        {
            return Result<OAuthState>.Failure(Error.Failure("Invalid oauth state"));
        }

        NonEmptyString? additionalState = null;

        if (parts.Length == 2)
        {
            var result = NonEmptyString.Create(parts[1]);
            if (result.IsSuccess)
            {
                additionalState = result.Value;
            }
        }

        return Result<OAuthState>.Success(new OAuthState(stateId, additionalState));
    }

    public override string ToString() =>
        AdditionalState is not null ? $"{StateId}{Delimiter}{AdditionalState.Value}" : StateId.ToString();
}