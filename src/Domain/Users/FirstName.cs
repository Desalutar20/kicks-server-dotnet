using Domain.Abstractions;
using Domain.Shared.ValueObjects;

namespace Domain.Users;

public sealed record FirstName : StringValueObject<FirstName>
{
    public const int MaxLength = 30;

    private FirstName(string value)
        : base(value) { }

    public static Result<FirstName> Create(string value) =>
        CreateCore(value, MaxLength, "firstName", "First name", (val) => new FirstName(val));

    public static implicit operator string(FirstName firstName) => firstName.Value;

    public static implicit operator FirstName(string value) => Create(value).Value;
}
