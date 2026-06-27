using Application.Admin.Products.Types;

namespace Application.Admin.Products.UseCases.GetProducts;

public sealed record GetProductsQuery(
    ProductFilters Filters,
    KeysetPagination<Guid> KeysetPagination
) : IQuery<KeysetPaginated<AdminProductResponse, Guid>>;

internal sealed class GetProductsQueryHandler(IProductReadRepository productReadRepository)
    : IQueryHandler<GetProductsQuery, KeysetPaginated<AdminProductResponse, Guid>>
{
    public async Task<Result<KeysetPaginated<AdminProductResponse, Guid>>> Handle(
        GetProductsQuery query,
        CancellationToken ct = default
    )
    {
        var data = await productReadRepository.GetProductsAsync(
            query.Filters,
            query.KeysetPagination,
            ct
        );

        return data;
    }
}
