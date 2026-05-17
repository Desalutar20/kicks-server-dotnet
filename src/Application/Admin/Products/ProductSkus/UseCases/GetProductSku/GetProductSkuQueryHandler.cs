using Application.Admin.Products.ProductSkus.Errors;
using Domain.Product.ProductSku;

namespace Application.Admin.Products.ProductSkus.UseCases.GetProductSku;

public sealed record GetProductSkuQuery(ProductSkuId Id) : IQuery<ProductSku>;

internal sealed class GetProductSkuQueryHandler(IProductSkusRepository productSkusRepository)
    : IQueryHandler<GetProductSkuQuery, ProductSku>
{
    public async Task<Result<ProductSku>> Handle(
        GetProductSkuQuery query,
        CancellationToken ct = default
    )
    {
        var data = await productSkusRepository.GetProductSkuByIdAsync(query.Id, false, ct);

        return data is null
            ? Result<ProductSku>.Failure(AdminProductSkuErrors.ProductSkuNotFound(query.Id))
            : Result<ProductSku>.Success(data);
    }
}
