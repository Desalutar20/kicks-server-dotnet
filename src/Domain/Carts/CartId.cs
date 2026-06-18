namespace Domain.Carts;

public sealed record CartId(Guid Value)
{
    public static implicit operator Guid(CartId userId) => userId.Value;

    public override string ToString() => Value.ToString();
}
