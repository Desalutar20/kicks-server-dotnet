using Domain.Abstractions;
using Domain.Shared.ValueObjects;

namespace Domain.DeliveryOptions;

public sealed record DeliveryOptionTitle : StringValueObject<DeliveryOptionTitle>
{
    public const int MaxLength = 60;

    private DeliveryOptionTitle(string value)
        : base(value) { }

    public static Result<DeliveryOptionTitle> Create(string value) =>
        CreateCore(
            value,
            MaxLength,
            "title",
            "Delivery option title",
            (val) => new DeliveryOptionTitle(val)
        );

    public static implicit operator string(DeliveryOptionTitle title) => title.Value;
}
