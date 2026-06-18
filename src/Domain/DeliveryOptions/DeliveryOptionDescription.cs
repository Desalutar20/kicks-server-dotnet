using Domain.Abstractions;
using Domain.Shared.ValueObjects;

namespace Domain.DeliveryOptions;

public sealed record DeliveryOptionDescription : StringValueObject<DeliveryOptionDescription>
{
    public const int MaxLength = 100;

    private DeliveryOptionDescription(string value)
        : base(value) { }

    public static Result<DeliveryOptionDescription> Create(string value) =>
        CreateCore(
            value,
            MaxLength,
            "description",
            "Delivery option description",
            (v) => new DeliveryOptionDescription(v)
        );
}
