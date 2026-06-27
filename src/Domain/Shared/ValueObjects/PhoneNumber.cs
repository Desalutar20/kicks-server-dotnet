using System.Text.RegularExpressions;
using Domain.Abstractions;

namespace Domain.Shared.ValueObjects;

public sealed record PhoneNumber : StringValueObject<PhoneNumber>
{
    public const int MaxLength = 100;

    private static readonly Regex Regex = new(
        @"^\+1\d{10}$",
        RegexOptions.Compiled,
        TimeSpan.FromMilliseconds(100)
    );

    private PhoneNumber(string value)
        : base(value) { }

    public static Result<PhoneNumber> Create(string value)
    {
        var result = CreateCore(
            value,
            MaxLength,
            "phoneNumber",
            "Phone number",
            (val) => new PhoneNumber(val)
        );
        if (result.IsFailure)
            return result.Error;

        if (!Regex.IsMatch(result.Value.Value))
        {
            return Error.Validation("phoneNumber", ["Phone number format is invalid"]);
        }

        return result.Value;
    }

    public static implicit operator string(PhoneNumber phoneNumber) => phoneNumber.Value;
}
