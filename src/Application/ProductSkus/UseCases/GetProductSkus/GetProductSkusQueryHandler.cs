using Application.Admin.Products.ProductSkus;
using Application.ProductSkus.Types;

namespace Application.ProductSkus.UseCases.GetProductSkus;

public sealed record GetProductSkusQuery(
    ProductSkusFilters Filters,
    KeysetPagination<Guid> KeysetPagination
) : IQuery<KeysetPaginated<ProductSkuListItemResponse, Guid>>;

internal sealed class GetProductSkusQueryHandler(
    IProductSkusReadRepository productSkusReadRepository
) : IQueryHandler<GetProductSkusQuery, KeysetPaginated<ProductSkuListItemResponse, Guid>>
{
    public async Task<Result<KeysetPaginated<ProductSkuListItemResponse, Guid>>> Handle(
        GetProductSkusQuery query,
        CancellationToken ct = default
    )
    {
        var data = await productSkusReadRepository.GetProductSkusAsync(
            query.Filters,
            query.KeysetPagination,
            ct
        );

        return data;
    }
}
