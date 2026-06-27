using Application.Admin.Products.ProductSkus.Errors;
using Application.Admin.Products.ProductSkus.Types;
using Application.ProductSkus;

namespace Application.Admin.Products.ProductSkus.UseCases.GetAdminProductSku;

public sealed record GetAdminProductSkuQuery(ProductSkuId Id) : IQuery<AdminProductSkuResponse>;

internal sealed class GetAdminProductSkuQueryHandler(
    IProductSkusReadRepository productSkusReadRepository
) : IQueryHandler<GetAdminProductSkuQuery, AdminProductSkuResponse>
{
    public async Task<Result<AdminProductSkuResponse>> Handle(
        GetAdminProductSkuQuery query,
        CancellationToken ct = default
    )
    {
        var data = await productSkusReadRepository.GetAdminProductSkuByIdAsync(query.Id, ct);

        return data is null ? AdminProductSkuErrors.ProductSkuNotFound(query.Id) : data;
    }
}
