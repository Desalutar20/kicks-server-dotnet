namespace Domain.Product;

public sealed record ProductId(Guid Value)
{
    public static implicit operator Guid(ProductId userId) => userId.Value;

    public override string ToString() => Value.ToString();
}
