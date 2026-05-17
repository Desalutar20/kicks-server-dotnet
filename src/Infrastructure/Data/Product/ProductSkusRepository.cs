using Domain.Product;
using Domain.Product.ProductSku;
using Infrastructure.Data.Extensions;

namespace Infrastructure.Data.Product;

internal sealed class ProductSkusRepository(AppDbContext dbContext)
    : RepositoryBase<ProductSku>(dbContext),
        IProductSkusRepository
{
    private readonly AppDbContext _dbContext = dbContext;

    public void CreateProductSku(ProductSku product) => Create(product);

    public void DeleteProductSku(ProductSku product) => Delete(product);

    public Task<bool> ExistsByProductSizeColorAsync(
        ProductId productId,
        PositiveInt size,
        ProductSkuColor color,
        CancellationToken ct = default
    ) =>
        _dbContext.ProductSkus.AnyAsync(
            x => x.ProductId == productId && x.Size == size && x.Color == color,
            ct
        );

    public async Task<bool> ExistsBySkuAsync(ProductSkuSku sku, CancellationToken ct = default) =>
        await _dbContext.ProductSkus.AnyAsync(x => x.Sku == sku, ct);

    public async Task<ProductSku?> GetProductSkuByIdAsync(
        ProductSkuId id,
        bool trackChanges,
        CancellationToken ct = default
    ) =>
        await FindByCondition(x => x.Id == id, trackChanges)
            .Include(x => x.Product)
            .Include(x => x.ProductSkuImages)
            .FirstOrDefaultAsync(ct);

    public async Task<KeysetPaginated<ProductSku, ProductSkuId>> GetProductSkusAsync(
        ProductSkusFilters filters,
        KeysetPagination<ProductSkuId> keysetPagination,
        bool trackChanges,
        CancellationToken ct = default
    )
    {
        var query = _dbContext.ProductSkus.AsQueryable();

        if (!trackChanges)
        {
            query = query.AsNoTracking();
        }

        query = query
            .WhereNotNull(
                filters.InStock,
                p => filters.InStock == true ? p.Quantity > 0 : p.Quantity == 0
            )
            .WhereNotNull(filters.MinPrice, p => p.Price.Price >= filters.MinPrice)
            .WhereNotNull(filters.MaxPrice, p => p.Price.Price <= filters.MaxPrice)
            .WhereNotNull(
                filters.MinSalePrice,
                p =>
                    p.Price.SalePrice != null
                    && p.Price.SalePrice.Value >= filters.MinSalePrice!.Value
            )
            .WhereNotNull(
                filters.MaxSalePrice,
                p =>
                    p.Price.SalePrice != null
                    && p.Price.SalePrice.Value <= filters.MaxSalePrice!.Value
            )
            .WhereNotNull(filters.Size, p => p.Size == filters.Size)
            .WhereNotNull(filters.Color, p => p.Color == filters.Color)
            .WhereNotNull(filters.Sku, p => p.Sku == filters.Sku)
            .ApplyKeysetPagination(keysetPagination)
            .Include(p => p.Product)
            .Include(p => p.ProductSkuImages);

        var result = await query.ToListAsync(ct);

        return new KeysetPaginated<ProductSku, ProductSkuId>(
            result,
            keysetPagination,
            u => u.CreatedAt,
            u => u.Id
        );
    }

    public void UpdateProductSku(ProductSku product) => Update(product);
}
