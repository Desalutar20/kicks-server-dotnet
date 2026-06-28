using Application.Admin.Products.Types;
using Application.Shared.Types;

namespace Application.Admin.Products.ProductSkus.Types;

public sealed record AdminProductSkuResponse
{
    public Guid Id { get; init; }
    public DateTimeOffset CreatedAt { get; init; }
    public DateTimeOffset UpdatedAt { get; init; }
    public decimal Price { get; init; }
    public decimal? SalePrice { get; init; }
    public int Quantity { get; init; }
    public required int Size { get; init; }
    public required string Color { get; init; }
    public required string Sku { get; init; }
    public required AdminProductResponse Product { get; init; }
    public List<FileResponse> Images { get; init; } = [];
}

internal static class AdminProductSkuResponseMapper
{
    public static AdminProductSkuResponse ToAdminResponse(this ProductSku productSku)
    {
        return new AdminProductSkuResponse()
        {
            Id = productSku.Id.Value,
            CreatedAt = productSku.CreatedAt,
            UpdatedAt = productSku.UpdatedAt,
            Price = productSku.Price.Price.Dollars,
            SalePrice = productSku.Price.SalePrice?.Dollars,
            Quantity = productSku.Quantity,
            Size = productSku.Size.Value,
            Color = productSku.Color.Value,
            Sku = productSku.Sku.Value,
            Product = productSku.Product.ToAdminResponse(),
            Images = productSku
                .Images.Select(image => new FileResponse()
                {
                    Id = image.Id,
                    Url = image.Url.Value,
                    Name = image.Name.FullName,
                })
                .ToList(),
        };
    }
}
