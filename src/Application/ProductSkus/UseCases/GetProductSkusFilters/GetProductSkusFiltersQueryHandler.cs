using Domain.Product.ProductSku;

namespace Application.ProductSkus.UseCases.GetProductSkusFilters;

public sealed record GetProductSkusFiltersQuery : IQuery<ProductSkusFilterOptions>;

internal sealed class GetProductSkusFiltersQueryHandler(
    IProductSkusRepository productSkusRepository
) : IQueryHandler<GetProductSkusFiltersQuery, ProductSkusFilterOptions>
{
    public async Task<Result<ProductSkusFilterOptions>> Handle(
        GetProductSkusFiltersQuery query,
        CancellationToken ct = default
    )
    {
        var filters = await productSkusRepository.GetProductSkusFilterOptions(false, ct);

        return Result<ProductSkusFilterOptions>.Success(filters);
    }
}
