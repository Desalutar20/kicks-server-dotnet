using Domain.Abstractions;

namespace Domain.Shared.Pagination;

public sealed record KeysetCursor<T>(DateTimeOffset CreatedAt, T Id)
{
    private const char CursorSeparator = '|';

    public static Result<KeysetCursor<T>> Create(string value, Func<string, Result<T>> parse)
    {
        var errors = new List<string>();

        string decoded;

        try
        {
            decoded = System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(value));
        }
        catch (Exception)
        {
            return Error.Validation("cursor", ["Invalid cursor"]);
        }

        var parts = decoded.Split(CursorSeparator);
        if (parts.Length != 2)
        {
            return Error.Validation("cursor", ["Invalid cursor"]);
        }

        if (
            !DateTimeOffset.TryParseExact(
                parts[0],
                "O",
                null,
                System.Globalization.DateTimeStyles.RoundtripKind,
                out var date
            )
        )
        {
            errors.Add("Invalid cursor date format");
        }

        var parsedResult = parse(parts[1]);
        if (parsedResult.IsFailure)
        {
            errors.Add("Invalid cursor id format");
        }

        return errors.Count == 0
            ? new KeysetCursor<T>(date, parsedResult.Value)
            : Error.Validation("cursor", errors);
    }

    public override string ToString()
    {
        var fullValue = $"{CreatedAt:O}{CursorSeparator}{Id}";
        var plainTextBytes = System.Text.Encoding.UTF8.GetBytes(fullValue);

        return Convert.ToBase64String(plainTextBytes);
    }
}
