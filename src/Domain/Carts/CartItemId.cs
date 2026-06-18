namespace Domain.Carts;

public sealed record CartItemId(Guid Value)
{
    public static implicit operator Guid(CartItemId userId) => userId.Value;

    public override string ToString() => Value.ToString();
}
