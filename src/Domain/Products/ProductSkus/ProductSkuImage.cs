using System.Text.Json.Serialization;
using Domain.Shared.FileContent;

namespace Domain.Products.ProductSkus;

public sealed record ProductSkuImage : FileContent
{
    [JsonIgnore]
    public ProductSkuId? ProductSkuId { get; private set; }

    private ProductSkuImage() { }

    public static ProductSkuImage Create(Guid imageId, FileUrl url, FileName name) =>
        new()
        {
            Id = imageId,
            Url = url,
            Name = name,
        };
}
