using Application.ProductSkus.Types;

namespace Application.ProductSkus.UseCases.GetProductSku;

public sealed record GetProductSkuQuery(ProductSkuId Id) : IQuery<ProductSkuWithVariants>;

internal sealed class GetProductSkuQueryHandler(IProductSkusRepository productSkusRepository)
    : IQueryHandler<GetProductSkuQuery, ProductSkuWithVariants>
{
    public async Task<Result<ProductSkuWithVariants>> Handle(
        GetProductSkuQuery query,
        CancellationToken ct = default
    )
    {
        var data = await productSkusRepository.GetProductSkuByIdAsync(query.Id, false, ct);
        if (data is null)
        {
            return Error.Failure($"Product sku with id '{query.Id}' doesn't exist");
        }

        var skus = await productSkusRepository.GetVariants(data.ProductId, false, ct);

        var variants = skus.GroupBy(x => x.Size)
            .Select(g =>
            {
                var matchingColor = g.FirstOrDefault(p => p.Color == data.Color && p.Quantity > 0);

                var selected = matchingColor ?? g.FirstOrDefault(p => p.Quantity > 0) ?? g.First();

                return new ProductSkuVariant(
                    g.Key,
                    selected.Id,
                    selected.Quantity > 0,
                    g.Select(x => new ProductSkuVariantColor(x.Color, x.Id, x.Quantity > 0))
                        .ToList()
                );
            })
            .ToList();

        return new ProductSkuWithVariants(data, variants);
    }
}
