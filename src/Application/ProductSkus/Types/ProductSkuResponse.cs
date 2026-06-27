using Application.Shared.Types;

namespace Application.ProductSkus.Types;

public sealed record ProductSkuResponse
{
    public Guid Id { get; init; }
    public DateTimeOffset CreatedAt { get; init; }
    public decimal Price { get; init; }
    public decimal? SalePrice { get; init; }
    public int Quantity { get; init; }
    public required int Size { get; init; }
    public required string Color { get; init; }
    public required string Sku { get; init; }
    public required ProductResponse Product { get; init; }
    public List<FileResponse> Images { get; init; } = [];
}
