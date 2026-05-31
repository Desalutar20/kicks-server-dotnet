using Domain.Abstractions;
using Domain.Shared.ValueObjects;

namespace Domain.Users;

public sealed record LastName : StringValueObject<LastName>
{
    public const int MaxLength = 30;

    private LastName(string value)
        : base(value) { }

    public static Result<LastName> Create(string value) =>
        CreateCore(value, MaxLength, "lastName", "Last name", (val) => new LastName(val));

    public static implicit operator string(LastName firstName) => firstName.Value;

    public static implicit operator LastName(string value) => Create(value).Value;
}
