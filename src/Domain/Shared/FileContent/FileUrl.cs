using Domain.Abstractions;

namespace Domain.Shared.FileContent;

public sealed record FileUrl
{
    public const int MaxLength = 200;

    private FileUrl(Uri value)
    {
        Value = value;
    }

    public Uri Value { get; }

    public static Result<FileUrl> Create(string value)
    {
        var errors = new List<string>();

        var emptyResult = Guard.AgainstEmptyString(value, "File url");
        if (emptyResult.IsFailure)
        {
            errors.Add(emptyResult.Error.Description);
        }

        value = value.Trim();

        var lengthResult = Guard.ForStringLength(value, 1, MaxLength, "File url");
        if (lengthResult.IsFailure)
        {
            errors.Add(lengthResult.Error.Description);
        }

        if (
            !Uri.TryCreate(value, UriKind.Absolute, out var uri)
            || (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps)
        )
        {
            errors.Add("File URL must be a valid HTTP or HTTPS URL.");
        }

        return errors.Count == 0 ? new FileUrl(uri!) : Error.Validation("fileUrl", errors);
    }
};
