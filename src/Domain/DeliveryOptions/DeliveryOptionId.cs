namespace Domain.DeliveryOptions;

public sealed record DeliveryOptionId(Guid Value)
{
    public static implicit operator Guid(DeliveryOptionId userId) => userId.Value;

    public override string ToString() => Value.ToString();
}
