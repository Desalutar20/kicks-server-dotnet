using Application.Admin.Products.Types;

namespace Application.Admin.Products.UseCases.GetProductFilters;

public sealed record GetProductFiltersQuery : IQuery<ProductFilterOptions>;

internal sealed class GetProductFiltersQueryHandler(IProductReadRepository productReadRepository)
    : IQueryHandler<GetProductFiltersQuery, ProductFilterOptions>
{
    public async Task<Result<ProductFilterOptions>> Handle(
        GetProductFiltersQuery query,
        CancellationToken ct = default
    )
    {
        var filters = await productReadRepository.GetProductsFilterOptions(ct);

        return filters;
    }
}
