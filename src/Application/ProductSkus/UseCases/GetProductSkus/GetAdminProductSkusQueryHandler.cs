namespace Application.ProductSkus.UseCases.GetProductSkus;

public sealed record GetProductSkusQuery(
    ProductSkusFilters Filters,
    KeysetPagination<ProductSkuId> KeysetPagination
) : IQuery<KeysetPaginated<ProductSku, ProductSkuId>>;

internal sealed class GetProductSkusQueryHandler(IProductSkusRepository productSkusRepository)
    : IQueryHandler<GetProductSkusQuery, KeysetPaginated<ProductSku, ProductSkuId>>
{
    public async Task<Result<KeysetPaginated<ProductSku, ProductSkuId>>> Handle(
        GetProductSkusQuery query,
        CancellationToken ct = default
    )
    {
        var data = await productSkusRepository.GetProductSkusAsync(
            query.Filters,
            query.KeysetPagination,
            false,
            ct
        );

        return data;
    }
}
