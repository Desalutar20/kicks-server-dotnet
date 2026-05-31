using Domain.Abstractions;
using Domain.Shared.ValueObjects;

namespace Domain.Users;

public sealed record Password : StringValueObject<Password>
{
    public const int MinLength = 8;
    public const int MaxLength = 40;

    private Password(string value)
        : base(value) { }

    public static Result<Password> Create(string value) =>
        CreateCore(
            value,
            MaxLength,
            "password",
            "Password",
            (val) => new Password(val),
            minLength: MinLength
        );

    public override string ToString() => Value;
}
