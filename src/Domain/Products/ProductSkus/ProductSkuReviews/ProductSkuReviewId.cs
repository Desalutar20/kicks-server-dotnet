namespace Domain.Products.ProductSkus.ProductSkuReviews;

public sealed record ProductSkuReviewId(Guid Value)
{
    public static implicit operator Guid(ProductSkuReviewId userId) => userId.Value;

    public override string ToString() => Value.ToString();
}
