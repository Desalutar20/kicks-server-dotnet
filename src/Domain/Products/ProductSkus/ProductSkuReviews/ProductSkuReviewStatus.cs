using System.Text.Json.Serialization;

namespace Domain.Products.ProductSkus.ProductSkuReviews;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum ProductSkuReviewStatus
{
    Pending,
    Rejected,
    Approved,
}
