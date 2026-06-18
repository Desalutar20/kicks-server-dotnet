namespace Application.Admin.Products.UseCases.GetProductFilters;

public sealed record GetProductFiltersQuery : IQuery<ProductFilterOptions>;

internal sealed class GetProductFiltersQueryHandler(IProductRepository productRepository)
    : IQueryHandler<GetProductFiltersQuery, ProductFilterOptions>
{
    public async Task<Result<ProductFilterOptions>> Handle(
        GetProductFiltersQuery query,
        CancellationToken ct = default
    )
    {
        var filters = await productRepository.GetProductsFilterOptions(ct);

        return filters;
    }
}
