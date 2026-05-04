namespace Domain.Product.ProductSku;

public sealed record ProductSkuId(Guid Value)
{
    public static implicit operator Guid(ProductSkuId userId) => userId.Value;

    public override string ToString() => Value.ToString();
}
