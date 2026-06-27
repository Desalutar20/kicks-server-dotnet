namespace Infrastructure.Data.Product;

internal sealed class ProductRepository(AppDbContext dbContext)
    : RepositoryBase<DomainProduct>(dbContext),
        IProductRepository
{
    public void CreateProduct(DomainProduct product) => Create(product);

    public void DeleteProduct(DomainProduct product) => Delete(product);

    public async Task<DomainProduct?> GetProductByIdAsync(
        ProductId id,
        bool trackChanges,
        CancellationToken ct = default
    ) =>
        await FindByCondition(x => x.Id == id, trackChanges)
            .Include(x => x.Brand)
            .Include(x => x.Category)
            .FirstOrDefaultAsync(ct);

    public void UpdateProduct(DomainProduct product) => Update(product);
}
