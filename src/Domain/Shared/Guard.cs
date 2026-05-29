using Domain.Abstractions;

namespace Domain.Shared;

public static class Guard
{
    public static Result AgainstEmptyString(string value, string name = "Value") =>
        !string.IsNullOrWhiteSpace(value)
            ? Result.Success()
            : Error.Failure($"{name} cannot be empty.");

    public static Result ForStringLength(
        string value,
        int minLength,
        int maxLength,
        string name = "Value"
    )
    {
        if (value.Length < minLength || value.Length > maxLength)
            return Error.Failure(
                $"{name} must have between {minLength} and {maxLength} characters."
            );

        return Result.Success();
    }

    public static Result AgainstOutOfRange(int number, int min, int max, string name = "Value") =>
        number >= min && number <= max
            ? Result.Success()
            : Error.Failure($"{name} must be between {min} and {max}.");
}
