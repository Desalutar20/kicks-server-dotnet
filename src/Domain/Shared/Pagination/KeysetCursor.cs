using Domain.Abstractions;

namespace Domain.Shared.Pagination;

public sealed record KeysetCursor<T>(DateTimeOffset CreatedAt, T Id)
{
    private const char CursorSeparator = '|';

    public static Result<KeysetCursor<T>> Create(string value, Func<string, Result<T>> parse)
    {
        var errors = new List<string>();

        var decoded = System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(value));
        var parts = decoded.Split(CursorSeparator);
        if (parts.Length != 2)
        {
            return Result<KeysetCursor<T>>.Failure(Error.Validation("cursor", ["Invalid cursor"]));
        }

        if (!DateTimeOffset.TryParse(parts[0], out var date))
        {
            errors.Add("Invalid cursor date format");
        }

        var parsedResult = parse(parts[1]);
        if (parsedResult.IsFailure)
        {
            errors.Add("Invalid cursor id format");
        }

        return errors.Count == 0
            ? Result<KeysetCursor<T>>.Success(new KeysetCursor<T>(date, parsedResult.Value))
            : Result<KeysetCursor<T>>.Failure(Error.Validation("cursor", errors));
    }

    public override string ToString()
    {
        var fullValue = $"{CreatedAt}{CursorSeparator}{Id}";
        var plainTextBytes = System.Text.Encoding.UTF8.GetBytes(fullValue);

        return Convert.ToBase64String(plainTextBytes);
    }
}
