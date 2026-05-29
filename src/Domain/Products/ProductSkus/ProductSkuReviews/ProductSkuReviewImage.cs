using System.Text.Json.Serialization;
using Domain.Shared.FileContent;

namespace Domain.Products.ProductSkus.ProductSkuReviews;

public sealed record ProductSkuReviewImage : FileContent
{
    [JsonIgnore]
    public ProductSkuReviewId? ProductSkuReviewId { get; private set; }

    private ProductSkuReviewImage() { }

    public static ProductSkuReviewImage Create(Guid imageId, FileUrl url, FileName name) =>
        new()
        {
            Id = imageId,
            Url = url,
            Name = name,
        };
}
