using System.Text.Json;
using Domain.Product;
using Infrastructure.Data.Extensions;
using Infrastructure.Data.Product.JsonConverters;

namespace Infrastructure.Data.Product;

internal sealed class ProductRepository(AppDbContext dbContext)
    : RepositoryBase<DomainProduct>(dbContext),
        IProductRepository
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        Converters = { new BrandFilterItemConverter(), new CategoryFilterItemConverter() },
    };

    private readonly AppDbContext _dbContext = dbContext;

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

    public async Task<KeysetPaginated<DomainProduct, ProductId>> GetProductsAsync(
        ProductFilters filters,
        KeysetPagination<ProductId> keysetPagination,
        bool trackChanges,
        CancellationToken ct = default
    )
    {
        var query = _dbContext.Products.AsQueryable();

        if (!trackChanges)
        {
            query = query.AsNoTracking().Include(x => x.Brand).Include(x => x.Category);
        }

        var search = filters.Search?.Value.ToLower();

        query = query
            .WhereNotNull(
                search,
                p =>
                    ((string)p.Title).ToLower().StartsWith(search!)
                    || ((string)p.Description).ToLower().StartsWith(search!)
            )
            .WhereNotNull(filters.Gender, p => p.Gender == filters.Gender)
            .WhereNotNull(filters.IsDeleted, p => p.IsDeleted == filters.IsDeleted)
            .WhereNotNull(filters.BrandId, p => p.BrandId == filters.BrandId)
            .WhereNotNull(filters.CategoryId, p => p.CategoryId == filters.CategoryId)
            .ApplyKeysetPagination(keysetPagination);

        var result = await query.ToListAsync(ct);

        return new KeysetPaginated<DomainProduct, ProductId>(
            result,
            keysetPagination,
            u => u.CreatedAt,
            u => u.Id
        );
    }

    public async Task<ProductFilterOptions> GetProductsFilterOptions(
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
            ARRAY(
                SELECT DISTINCT tag
                FROM product,
                     UNNEST(tags) AS tag
            ) AS tags,

            (SELECT JSON_AGG(JSON_BUILD_OBJECT('id', c.id, 'name', c.name))
             FROM category c
             WHERE EXISTS (
                 SELECT 1 FROM product p WHERE p.category_id = c.id
             )) AS ""availableCategories"",

            (SELECT JSON_AGG(JSON_BUILD_OBJECT('id', c.id, 'name', c.name))
             FROM category c) AS categories,

            (SELECT JSON_AGG(JSON_BUILD_OBJECT('id', b.id, 'name', b.name))
             FROM brand b
             WHERE EXISTS (
                 SELECT 1 FROM product p WHERE p.brand_id = b.id
             )) AS ""availableBrands"",

            (SELECT JSON_AGG(JSON_BUILD_OBJECT('id', b.id, 'name', b.name))
             FROM brand b) AS brands
    ";

        await using var reader = await command.ExecuteReaderAsync(ct);

        if (!await reader.ReadAsync(ct))
            throw new InvalidOperationException(
                "Failed to load ProductFilterOptions: query returned no rows."
            );

        var result = new ProductFilterOptions(
            ParseTags(reader["tags"]),
            Deserialize<List<CategoryFilterItem>>(reader["categories"]) ?? [],
            Deserialize<List<CategoryFilterItem>>(reader["availableCategories"]) ?? [],
            Deserialize<List<BrandFilterItem>>(reader["brands"]) ?? [],
            Deserialize<List<BrandFilterItem>>(reader["availableBrands"]) ?? []
        );

        return result;
    }

    public void UpdateProduct(DomainProduct product) => Update(product);

    private static T? Deserialize<T>(object? value)
    {
        if (value is null || value is DBNull)
            return default!;

        var json = value switch
        {
            string s => s,
            JsonElement e => e.GetRawText(),
            _ => value.ToString(),
        };

        return JsonSerializer.Deserialize<T>(json!, JsonOptions)!;
    }

    private static string[] ParseTags(object? value)
    {
        if (value is null || value is DBNull)
            return [];

        if (value is string[] arr)
            return arr;

        return JsonSerializer.Deserialize<string[]>(value.ToString()!) ?? [];
    }
}
