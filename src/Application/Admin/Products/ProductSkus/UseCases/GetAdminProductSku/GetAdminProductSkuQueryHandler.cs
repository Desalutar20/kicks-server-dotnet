using Application.Admin.Products.ProductSkus.Errors;

namespace Application.Admin.Products.ProductSkus.UseCases.GetAdminProductSku;

public sealed record GetAdminProductSkuQuery(ProductSkuId Id) : IQuery<ProductSku>;

internal sealed class GetAdminProductSkuQueryHandler(IProductSkusRepository productSkusRepository)
    : IQueryHandler<GetAdminProductSkuQuery, ProductSku>
{
    public async Task<Result<ProductSku>> Handle(
        GetAdminProductSkuQuery query,
        CancellationToken ct = default
    )
    {
        var data = await productSkusRepository.GetProductSkuByIdAsync(query.Id, false, ct);

        return data is null ? AdminProductSkuErrors.ProductSkuNotFound(query.Id) : data;
    }
}
