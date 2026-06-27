using Application.Shared.Types;

namespace Application.ProductSkus.Types;

public sealed record ProductSkuListItemResponse
{
    public Guid Id { get; init; }
    public DateTimeOffset CreatedAt { get; init; }

    public decimal Price { get; init; }
    public decimal? SalePrice { get; init; }
    public int Quantity { get; init; }

    public required string Title { get; init; }
    public List<FileResponse> Images { get; init; } = [];

    public Guid? CategoryId { get; init; }
}
