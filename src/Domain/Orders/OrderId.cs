namespace Domain.Orders;

public sealed record OrderId(Guid Value)
{
    public static implicit operator Guid(OrderId userId) => userId.Value;

    public override string ToString() => Value.ToString();
}
