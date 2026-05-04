namespace Domain.Product.ProductSku.ProductSkuImage;

public sealed record ProductSkuImageId(Guid Value)
{
    public static implicit operator Guid(ProductSkuImageId userId) => userId.Value;

    public override string ToString() => Value.ToString();
}
