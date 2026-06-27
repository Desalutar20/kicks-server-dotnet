using Application.Admin.Products.ProductSkus.Types;
using Application.ProductSkus;

namespace Application.Admin.Products.ProductSkus.UseCases.GetAdminProductSkus;

public sealed record GetAdminProductSkusQuery(
    AdminProductSkusFilters Filters,
    KeysetPagination<Guid> KeysetPagination
) : IQuery<KeysetPaginated<AdminProductSkuResponse, Guid>>;

internal sealed class GetAdminProductSkusQueryHandler(
    IProductSkusReadRepository productSkusReadRepository
) : IQueryHandler<GetAdminProductSkusQuery, KeysetPaginated<AdminProductSkuResponse, Guid>>
{
    public async Task<Result<KeysetPaginated<AdminProductSkuResponse, Guid>>> Handle(
        GetAdminProductSkusQuery query,
        CancellationToken ct = default
    )
    {
        var data = await productSkusReadRepository.GetAdminProductSkusAsync(
            query.Filters,
            query.KeysetPagination,
            ct
        );

        return data;
    }
}
