using Domain.Shared.ValueObjects;
using Infrastructure.Data.Extensions;

namespace Infrastructure.Data.Product;

internal sealed class ProductSkusRepository(AppDbContext dbContext)
    : RepositoryBase<ProductSku>(dbContext),
        IProductSkusRepository
{
    private readonly AppDbContext _dbContext = dbContext;

    public void CreateProductSku(ProductSku product) => Create(product);

    public void DeleteProductSku(ProductSku product) => Delete(product);

    public async Task<ProductSku?> GetProductSkuByIdAsync(
        ProductSkuId id,
        bool trackChanges,
        CancellationToken ct = default
    ) => await FindByCondition(x => x.Id == id, trackChanges).FirstOrDefaultAsync(ct);

    public async Task<KeysetPaginated<ProductSku, ProductSkuId>> GetAdminProductSkusAsync(
        AdminProductSkusFilters filters,
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
            .WhereNotNull(filters.MinPrice, p => p.Price.Price >= filters.MinPrice!)
            .WhereNotNull(filters.MaxPrice, p => p.Price.Price <= filters.MaxPrice!)
            .WhereNotNull(
                filters.MinSalePrice,
                p => p.Price.SalePrice != null && p.Price.SalePrice >= filters.MinSalePrice!
            )
            .WhereNotNull(
                filters.MaxSalePrice,
                p => p.Price.SalePrice != null && p.Price.SalePrice <= filters.MaxSalePrice!
            )
            .WhereNotNull(filters.Size, p => p.Size == filters.Size)
            .WhereNotNull(filters.Color, p => p.Color == filters.Color)
            .WhereNotNull(filters.Sku, p => p.Sku == filters.Sku)
            .ApplyKeysetPagination(keysetPagination);

        var result = await query.ToListAsync(ct);

        return new KeysetPaginated<ProductSku, ProductSkuId>(
            result,
            keysetPagination,
            u => u.CreatedAt,
            u => u.Id
        );
    }

    public async Task BulkIncrementQuantityAsync(
        IEnumerable<(ProductSkuId Id, PositiveInt Quantity)> data,
        CancellationToken ct = default
    )
    {
        if (!data.Any())
            return;

        var items = data.ToArray();

        var valuesSql = string.Join(",", items.Select((_, i) => $"(@id{i}, @qty{i})"));

        var parameters = new List<Npgsql.NpgsqlParameter>(items.Length * 2);

        for (var i = 0; i < items.Length; i++)
        {
            parameters.Add(new Npgsql.NpgsqlParameter($"id{i}", items[i].Id.Value));
            parameters.Add(new Npgsql.NpgsqlParameter($"qty{i}", items[i].Quantity.Value));
        }

        var sql = $"""
                UPDATE product_sku ps
                SET quantity = ps.quantity + v.qty
                FROM (VALUES {valuesSql}) AS v(id, qty)
                WHERE ps.id = v.id;
            """;

        await _dbContext.Database.ExecuteSqlRawAsync(sql, parameters, ct);
    }

    public void UpdateProductSku(ProductSku product) => Update(product);
}
