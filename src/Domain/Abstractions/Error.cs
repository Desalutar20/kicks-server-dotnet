namespace Domain.Abstractions;

public sealed record Error
{
    private Error(
        string message,
        ErrorType errorType,
        (string, IEnumerable<string>)? errors = null)
    {
        Description = message;
        ErrorType = errorType;
        Errors = errors;
    }

    public string Description { get; }
    public ErrorType ErrorType { get; }
    public (string, IEnumerable<string>)? Errors { get; }

    public static Error Failure(string message) => new(message, ErrorType.Failure);
    public static Error NotFound(string message) => new(message, ErrorType.NotFound);

    public static Error Validation(string field, IEnumerable<string> errors) =>
        new("Validation error", ErrorType.Validation,
            (field, errors));

    public static Error Conflict(string message) => new(message, ErrorType.Conflict);
    public static Error Unauthorized(string message) => new(message, ErrorType.Unauthorized);
    public static Error AccessForbidden(string message) => new(message, ErrorType.AccessForbidden);
    public static Error Internal(string message) => new(message, ErrorType.Internal);
}