using Application.ProductSkus.Types;

namespace Application.ProductSkus.UseCases.GetProductSkusFilters;

public sealed record GetProductSkusFiltersQuery : IQuery<ProductSkusFilterOptions>;

internal sealed class GetProductSkusFiltersQueryHandler(
    IProductSkusReadRepository productSkusReadRepository
) : IQueryHandler<GetProductSkusFiltersQuery, ProductSkusFilterOptions>
{
    public async Task<Result<ProductSkusFilterOptions>> Handle(
        GetProductSkusFiltersQuery query,
        CancellationToken ct = default
    )
    {
        var filters = await productSkusReadRepository.GetProductSkusFilterOptions(ct);
        // var filters = await productSkusRepository.GetProductSkusFilterOptions(ct);

        return filters;
    }
}
