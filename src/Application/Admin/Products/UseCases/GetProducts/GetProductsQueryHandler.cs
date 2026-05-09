namespace Application.Admin.Products.UseCases.GetProducts;

public sealed record GetProductsQuery(
    ProductFilters Filters,
    KeysetPagination<ProductId> KeysetPagination
) : IQuery<KeysetPaginated<Product, ProductId>>;

internal sealed class GetProductsQueryHandler(IProductRepository productRepository)
    : IQueryHandler<GetProductsQuery, KeysetPaginated<Product, ProductId>>
{
    public async Task<Result<KeysetPaginated<Product, ProductId>>> Handle(
        GetProductsQuery query,
        CancellationToken ct = default
    )
    {
        var data = await productRepository.GetProductsAsync(
            query.Filters,
            query.KeysetPagination,
            false,
            ct
        );

        return Result<KeysetPaginated<Product, ProductId>>.Success(data);
    }
}
