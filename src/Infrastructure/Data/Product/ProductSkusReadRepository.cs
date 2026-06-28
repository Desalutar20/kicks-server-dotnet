using System.Data;
using Application.Admin.Products.ProductSkus;
using Application.Admin.Products.ProductSkus.Types;
using Application.Admin.Products.Types;
using Application.ProductSkus;
using Application.ProductSkus.Types;
using Dapper;
using Domain.Shared.ValueObjects;
using Infrastructure.Data.Extensions;

namespace Infrastructure.Data.Product;

internal sealed class ProductSkusReadRepository(IDbConnection connection)
    : IProductSkusReadRepository
{
    private const string BaseSelect = """
            SELECT
                ps.id as Id,
                ps.created_at as CreatedAt,
                ps.price / 100 as Price,
                ps.sale_price / 100 as SalePrice,
                ps.quantity as Quantity,
                ps.size as Size,
                ps.color as Color,
                ps.sku as Sku,
                ps.images as Images,

                p.id as Id,
                p.title as Title,
                p.description as Description,
                p.gender as Gender,
                p.tags::text[] as Tags,
                p.brand_id as BrandId,
                p.category_id as CategoryId
            FROM product_sku ps
            JOIN product p ON p.id = ps.product_id
        """;

    private const string BaseAdminSelect = """
             SELECT
                ps.id as Id,
                ps.created_at as CreatedAt,
                ps.updated_at as UpdatedAt,
                ps.price / 100 as Price,
                ps.sale_price / 100 as SalePrice,
                ps.quantity as Quantity,
                ps.size as Size,
                ps.color as Color,
                ps.sku as Sku,
                ps.images as Images,

                p.id as Id,
                p.created_at as CreatedAt,
                p.updated_at as UpdatedAt,
                p.title as Title,
                p.description as Description,
                p.gender as Gender,
                p.tags::text[] as Tags,
                p.brand_id as BrandId,
                p.category_id as CategoryId,
                p.is_deleted as IsDeleted
            FROM product_sku ps
            JOIN product p ON p.id = ps.product_id
        """;

    public async Task<KeysetPaginated<ProductSkuListItemResponse, Guid>> GetProductSkusAsync(
        ProductSkusFilters filters,
        KeysetPagination<Guid> keysetPagination,
        CancellationToken ct = default
    )
    {
        var builder = new SqlBuilder();

        builder.WhereNotNull(
            filters.Sizes,
            "ps.size = ANY(@Sizes)",
            new { Sizes = filters.Sizes?.Select(x => x.Value).ToArray() }
        );

        builder.WhereNotNull(
            filters.Colors,
            "ps.color = ANY(@Colors)",
            new { Colors = filters.Colors?.Select(x => x.Value).ToArray() }
        );

        builder.WhereNotNull(
            filters.CategoryIds,
            "p.category_id = ANY(@CategoryIds)",
            new { CategoryIds = filters.CategoryIds?.Select(x => x.Value).ToArray() }
        );

        builder.WhereNotNull(
            filters.BrandIds,
            "p.brand_id = ANY(@BrandIds)",
            new { BrandIds = filters.BrandIds?.Select(x => x.Value).ToArray() }
        );

        builder.WhereNotNull(
            filters.Genders,
            "p.gender = ANY(@Genders)",
            new { Genders = filters.Genders?.Select(g => g.ToString().ToLower()).ToArray() }
        );

        builder.WhereNotNull(
            filters.MinPrice,
            "ps.price >= @MinPrice",
            new { MinPrice = filters.MinPrice?.Cents }
        );
        builder.WhereNotNull(
            filters.MaxPrice,
            "ps.price <= @MaxPrice",
            new { MaxPrice = filters.MaxPrice?.Cents }
        );
        builder.Where("p.is_deleted = false");

        builder.ApplyKeysetPagination(keysetPagination, "ps");

        var template = builder.AddTemplate(
            """
            SELECT
                ps.id,
                ps.created_at as CreatedAt,
                ps.price / 100 as Price,
                ps.sale_price / 100 as SalePrice,
                ps.quantity,
                ps.images,
                p.title,
                p.category_id as CategoryId
            FROM product_sku ps
            JOIN product p ON p.id = ps.product_id
            /**where**/
            /**orderby**/
            LIMIT @LimitPlusOne;
            """,
            new { LimitPlusOne = keysetPagination.Limit + 1 }
        );

        var items = await connection.QueryAsync<ProductSkuListItemResponse>(
            new CommandDefinition(template.RawSql, template.Parameters, cancellationToken: ct)
        );

        return new KeysetPaginated<ProductSkuListItemResponse, Guid>(
            items.ToList(),
            keysetPagination,
            response => response.CreatedAt,
            response => response.Id
        );
    }

    public async Task<KeysetPaginated<AdminProductSkuResponse, Guid>> GetAdminProductSkusAsync(
        AdminProductSkusFilters filters,
        KeysetPagination<Guid> keysetPagination,
        CancellationToken ct = default
    )
    {
        var builder = new SqlBuilder();

        builder.WhereNotNull(
            filters.InStock,
            filters.InStock is null ? ""
                : filters.InStock!.Value ? "ps.quantity > 0"
                : "ps.quantity = 0"
        );

        builder.WhereNotNull(
            filters.MinPrice,
            "ps.price >= @MinPrice",
            new { MinPrice = filters.MinPrice?.Cents }
        );

        builder.WhereNotNull(
            filters.MaxPrice,
            "ps.price <= @MaxPrice",
            new { MaxPrice = filters.MaxPrice?.Cents }
        );

        builder.WhereNotNull(
            filters.MinSalePrice,
            "ps.sale_price IS NOT NULL AND ps.sale_price >= @MinSalePrice",
            new { MinSalePrice = filters.MinSalePrice?.Cents }
        );

        builder.WhereNotNull(
            filters.MaxSalePrice,
            "ps.sale_price IS NOT NULL AND ps.sale_price <= @MaxSalePrice",
            new { MaxSalePrice = filters.MaxSalePrice?.Cents }
        );

        builder.WhereNotNull(filters.Size, "ps.size = @Size", new { Size = filters.Size?.Value });
        builder.WhereNotNull(
            filters.Color,
            "ps.color = @Color",
            new { Color = filters.Color?.Value }
        );
        builder.WhereNotNull(filters.Sku, "ps.sku = @Sku", new { Sku = filters.Sku?.Value });

        builder.ApplyKeysetPagination(keysetPagination, "ps");

        var template = builder.AddTemplate(
            BaseAdminSelect
                + """

                /**where**/
                /**orderby**/
                LIMIT @LimitPlusOne;
                """,
            new { LimitPlusOne = keysetPagination.Limit + 1 }
        );

        var command = new CommandDefinition(
            template.RawSql,
            template.Parameters,
            cancellationToken: ct
        );

        var items = await connection.QueryAsync<
            AdminProductSkuResponse,
            AdminProductResponse,
            AdminProductSkuResponse
        >(command, (sku, product) => sku with { Product = product }, splitOn: "Id");

        return new KeysetPaginated<AdminProductSkuResponse, Guid>(
            items.ToList(),
            keysetPagination,
            response => response.CreatedAt,
            response => response.Id
        );
    }

    public async Task<ProductSkuResponse?> GetProductSkuByIdAsync(
        ProductSkuId id,
        CancellationToken ct = default
    )
    {
        const string sql = BaseSelect + " WHERE ps.id = @Id";

        var command = new CommandDefinition(sql, new { Id = id.Value }, cancellationToken: ct);

        var item = await connection.QueryAsync<
            ProductSkuResponse,
            ProductResponse,
            ProductSkuResponse
        >(command, (sku, product) => sku with { Product = product }, splitOn: "Id");

        return item.FirstOrDefault();
    }

    public async Task<AdminProductSkuResponse?> GetAdminProductSkuByIdAsync(
        ProductSkuId id,
        CancellationToken ct = default
    )
    {
        const string sql = BaseAdminSelect + " WHERE ps.id = @Id";

        var command = new CommandDefinition(sql, new { Id = id.Value }, cancellationToken: ct);

        var item = await connection.QueryAsync<
            AdminProductSkuResponse,
            AdminProductResponse,
            AdminProductSkuResponse
        >(command, (sku, product) => sku with { Product = product }, splitOn: "Id");

        return item.FirstOrDefault();
    }

    public async Task<IReadOnlyList<ProductSkuResponse>> GetVariants(
        ProductId productId,
        CancellationToken ct = default
    )
    {
        const string sql =
            BaseSelect
            + """

                WHERE p.id = @ProductId
                ORDER BY ps.size
                """;

        var command = new CommandDefinition(
            sql,
            new { ProductId = productId.Value },
            cancellationToken: ct
        );

        var items = await connection.QueryAsync<
            ProductSkuResponse,
            ProductResponse,
            ProductSkuResponse
        >(command, (sku, product) => sku with { Product = product }, splitOn: "Id");

        return items.ToList();
    }

    public async Task<ProductSkusFilterOptions> GetProductSkusFilterOptions(
        CancellationToken ct = default
    )
    {
        const string sql = """
            SELECT
                (SELECT ARRAY_AGG(DISTINCT sku.size) FROM product_sku sku) AS Sizes,
                (SELECT ARRAY_AGG(DISTINCT sku.color) FROM product_sku sku) AS Colors,

                (SELECT ARRAY_AGG(DISTINCT product.gender::text) FROM product) AS Genders,

                (SELECT JSON_AGG(JSON_BUILD_OBJECT('Id', c.id, 'Name', c.name))
                FROM category c
                WHERE EXISTS (
                    SELECT 1 FROM product p WHERE p.category_id = c.id
                )
                ) AS Categories,

                (SELECT JSON_AGG(JSON_BUILD_OBJECT('Id', b.id, 'Name', b.name))
                FROM brand b
                WHERE EXISTS (
                    SELECT 1 FROM product p WHERE p.brand_id = b.id
                )
                ) AS Brands,

                (SELECT COALESCE(MIN(price), 0) / 100 FROM product_sku) AS MinPrice,
                (SELECT COALESCE(MAX(price), 0) / 100 FROM product_sku) AS MaxPrice;
            """;

        var row = await connection.QuerySingleAsync<ProductSkusFilterOptions>(
            new CommandDefinition(sql, cancellationToken: ct)
        );

        return row;
    }

    public async Task<bool> ExistsBySkuAsync(ProductSkuSku sku, CancellationToken ct = default)
    {
        const string sql = """
            SELECT EXISTS (
                SELECT 1
                FROM product_sku ps
                WHERE ps.sku = @Sku
            );
            """;

        var exists = await connection.QuerySingleAsync<bool>(
            new CommandDefinition(sql, new { Sku = sku.Value }, cancellationToken: ct)
        );

        return exists;
    }

    public async Task<bool> ExistsByProductSizeColorAsync(
        ProductId productId,
        PositiveInt size,
        ProductSkuColor color,
        CancellationToken ct = default
    )
    {
        const string sql = """
            SELECT EXISTS (
                SELECT 1
                FROM product_sku ps
                WHERE ps.product_id = @ProductId AND ps.size = @Size AND ps.color = @Color
            );
            """;

        var exists = await connection.QuerySingleAsync<bool>(
            new CommandDefinition(
                sql,
                new
                {
                    ProductId = productId.Value,
                    Size = size.Value,
                    Color = color.Value,
                },
                cancellationToken: ct
            )
        );

        return exists;
    }
}
