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

    public async Task<IEnumerable<ProductSku>> GetVariants(
        ProductId productId,
        bool trackChanges,
        CancellationToken ct = default
    ) =>
        trackChanges
            ? await _dbContext
                .ProductSkus.Where(p => p.ProductId == productId)
                .OrderBy(x => x.Size)
                .ToListAsync(ct)
            : await _dbContext
                .ProductSkus.Where(p => p.ProductId == productId)
                .OrderBy(x => x.Size)
                .AsNoTracking()
                .ToListAsync(ct);

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
            .WhereNotNull(filters.Sizes, p => filters.Sizes!.Contains(p.Size))
            .WhereNotNull(filters.Colors, p => filters.Colors!.Contains(p.Color))
            .WhereNotNull(
                filters.CategoryIds,
                p => filters.CategoryIds!.Contains(p.Product.CategoryId)
            )
            .WhereNotNull(filters.BrandIds, p => filters.BrandIds!.Contains(p.Product.BrandId))
            .WhereNotNull(filters.Genders, p => filters.Genders!.Contains(p.Product.Gender))
            .WhereNotNull(filters.MinPrice, p => p.Price.Price >= filters.MinPrice)
            .WhereNotNull(filters.MaxPrice, p => p.Price.Price <= filters.MaxPrice)
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

    public async Task<ProductSkusFilterOptions> GetProductSkusFilterOptions(
        bool trackChanges,
        CancellationToken ct = default
    )
    {
        await using var connection = _dbContext.Database.GetDbConnection();
        await connection.OpenAsync(ct);

        await using var command = connection.CreateCommand();
        command.CommandText =
            @"
      SELECT
          (SELECT ARRAY_AGG(DISTINCT sku.size)
          FROM product_sku sku) AS sizes,

          (SELECT ARRAY_AGG(DISTINCT sku.color)
          FROM product_sku sku) AS colors,

          (SELECT ARRAY_AGG(DISTINCT product.gender)
          FROM product) AS genders,

          (SELECT JSON_AGG(JSON_BUILD_OBJECT('id', c.id, 'name', c.name))
             FROM category c
             WHERE EXISTS (
                 SELECT 1 FROM product p WHERE p.category_id = c.id
          )) AS categories,

          (SELECT JSON_AGG(JSON_BUILD_OBJECT('id', b.id, 'name', b.name))
             FROM brand b
             WHERE EXISTS (
                 SELECT 1 FROM product p WHERE p.brand_id = b.id
             )) AS brands,

          (SELECT MIN(price) FROM product_sku) AS min_price,

          (SELECT MAX(price) FROM product_sku) AS max_price;
    ";

        await using var reader = await command.ExecuteReaderAsync(ct);

        if (!await reader.ReadAsync(ct))
            throw new InvalidOperationException(
                "Failed to load ProductFilterOptions: query returned no rows."
            );

        var result = new ProductSkusFilterOptions(
            ParseArray<int>(reader["sizes"]),
            ParseArray<string>(reader["colors"]),
            ParseArray<string>(reader["genders"])
                .Select(x => Enum.Parse<ProductGender>(x, true))
                .ToArray(),
            Deserialize<List<CategoryFilterItem>>(reader["categories"]) ?? [],
            Deserialize<List<BrandFilterItem>>(reader["brands"]) ?? [],
            PositiveInt
                .Create(Convert.ToInt32(reader["min_price"] is DBNull ? 1 : reader["min_price"]))
                .Value,
            PositiveInt
                .Create(Convert.ToInt32(reader["max_price"] is DBNull ? 100 : reader["max_price"]))
                .Value
        );

        return result;
    }

    public void UpdateProductSku(ProductSku product) => Update(product);
}
