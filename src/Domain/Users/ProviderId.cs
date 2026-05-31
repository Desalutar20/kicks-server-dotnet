using Domain.Abstractions;
using Domain.Shared.ValueObjects;

namespace Domain.Users;

public sealed record ProviderId : StringValueObject<ProviderId>
{
    public const int MaxLength = 100;

    private ProviderId(string value)
        : base(value) { }

    public static Result<ProviderId> Create(string value) =>
        CreateCore(value, MaxLength, "Provider id", "providerId", (val) => new ProviderId(val));

    public override string ToString() => Value;
}
