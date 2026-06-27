using System.Data;
using Application.Admin.Products;
using Application.Admin.Products.Types;
using Dapper;
using Infrastructure.Data.Extensions;

namespace Infrastructure.Data.Product;

internal sealed class ProductReadRepository(IDbConnection connection) : IProductReadRepository
{
    public async Task<ProductFilterOptions> GetProductsFilterOptions(CancellationToken ct = default)
    {
        const string sql = """
            SELECT
                ARRAY(
                    SELECT DISTINCT tag
                    FROM product,
                         UNNEST(tags) AS tag
                ) AS Tags,

                (
                    SELECT JSON_AGG(
                        JSON_BUILD_OBJECT(
                            'Id', c.id,
                            'Name', c.name
                        )
                    )
                    FROM category c
                    WHERE EXISTS (
                        SELECT 1
                        FROM product p
                        WHERE p.category_id = c.id
                    )
                ) AS "AvailableCategories",

                (
                    SELECT JSON_AGG(
                        JSON_BUILD_OBJECT(
                            'Id', c.id,
                            'Name', c.name
                        )
                    )
                    FROM category c
                ) AS Categories,

                (
                    SELECT JSON_AGG(
                        JSON_BUILD_OBJECT(
                            'Id', b.id,
                            'Name', b.name
                        )
                    )
                    FROM brand b
                    WHERE EXISTS (
                        SELECT 1
                        FROM product p
                        WHERE p.brand_id = b.id
                    )
                ) AS "AvailableBrands",

                (
                    SELECT JSON_AGG(
                        JSON_BUILD_OBJECT(
                            'Id', b.id,
                            'Name', b.name
                        )
                    )
                    FROM brand b
                ) AS Brands;
            """;

        return await connection.QuerySingleAsync<ProductFilterOptions>(
            new CommandDefinition(sql, cancellationToken: ct)
        );
    }

    public async Task<KeysetPaginated<AdminProductResponse, Guid>> GetProductsAsync(
        ProductFilters filters,
        KeysetPagination<Guid> keysetPagination,
        CancellationToken ct = default
    )
    {
        var builder = new SqlBuilder();
        var search = filters.Search?.Value.ToLower();

        builder.WhereNotNull(
            search,
            "(p.title ILIKE @Search OR p.description ILIKE @Search)",
            new { Search = $"%{search}%" }
        );

        builder.WhereNotNull(
            filters.Gender,
            "p.gender = @Gender",
            new { Gender = filters.Gender.ToString()?.ToLower() }
        );

        builder.WhereNotNull(
            filters.IsDeleted,
            "p.is_deleted = @IsDeleted",
            new { filters.IsDeleted }
        );

        builder.WhereNotNull(filters.BrandId, "p.brand_id = @BrandId", new { filters.BrandId });

        builder.WhereNotNull(
            filters.CategoryId,
            "p.category_id = @CategoryId",
            new { filters.CategoryId }
        );

        builder.ApplyKeysetPagination(keysetPagination, "p");

        var template = builder.AddTemplate(
            """
                SELECT id as Id,
                       created_at as CreatedAt,
                       updated_at as UpdatedAt,
                       title,
                       description,
                       gender,
                       tags,
                       brand_id as BrandId,
                       category_id as CategoryId,
                       is_deleted as IsDeleted
                FROM product p
                /**where**/
                /**orderby**/
                LIMIT @LimitPlusOne;
            """,
            new { LimitPlusOne = keysetPagination.Limit + 1 }
        );

        var items = await connection.QueryAsync<AdminProductResponse>(
            new CommandDefinition(template.RawSql, template.Parameters, cancellationToken: ct)
        );

        return new KeysetPaginated<AdminProductResponse, Guid>(
            items.ToList(),
            keysetPagination,
            response => response.CreatedAt,
            response => response.Id
        );
    }
}
