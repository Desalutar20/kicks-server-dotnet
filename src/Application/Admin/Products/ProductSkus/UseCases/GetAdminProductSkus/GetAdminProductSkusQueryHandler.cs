namespace Application.Admin.Products.ProductSkus.UseCases.GetAdminProductSkus;

public sealed record GetAdminProductSkusQuery(
    AdminProductSkusFilters Filters,
    KeysetPagination<ProductSkuId> KeysetPagination
) : IQuery<KeysetPaginated<ProductSku, ProductSkuId>>;

internal sealed class GetAdminProductSkusQueryHandler(IProductSkusRepository productSkusRepository)
    : IQueryHandler<GetAdminProductSkusQuery, KeysetPaginated<ProductSku, ProductSkuId>>
{
    public async Task<Result<KeysetPaginated<ProductSku, ProductSkuId>>> Handle(
        GetAdminProductSkusQuery query,
        CancellationToken ct = default
    )
    {
        var data = await productSkusRepository.GetAdminProductSkusAsync(
            query.Filters,
            query.KeysetPagination,
            false,
            ct
        );

        return Result<KeysetPaginated<ProductSku, ProductSkuId>>.Success(data);
    }
}
