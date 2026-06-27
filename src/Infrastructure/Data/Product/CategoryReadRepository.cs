using System.Data;
using Application.Admin.Categories;
using Application.Admin.Categories.Types;
using Dapper;
using Domain.Shared.ValueObjects;
using Infrastructure.Data.Extensions;

namespace Infrastructure.Data.Product;

internal sealed class CategoryReadRepository(IDbConnection connection) : ICategoryReadRepository
{
    public async Task<KeysetPaginated<AdminCategoryResponse, Guid>> GetCategoriesAsync(
        NonEmptyString? search,
        KeysetPagination<Guid> keysetPagination,
        CancellationToken ct = default
    )
    {
        var builder = new SqlBuilder();
        var searchTerm = search?.Value.ToLower();

        builder.WhereNotNull(search, "c.name ILIKE @Search", new { Search = $"%{searchTerm}%" });
        builder.ApplyKeysetPagination(keysetPagination, "c");

        var template = builder.AddTemplate(
            """
                SELECT id as Id,
                       created_at as CreatedAt,
                       updated_at as UpdatedAt,
                       name as Name
                FROM category c
                /**where**/
                /**orderby**/
                LIMIT @LimitPlusOne;
            """,
            new { LimitPlusOne = keysetPagination.Limit + 1 }
        );

        var items = await connection.QueryAsync<AdminCategoryResponse>(
            new CommandDefinition(template.RawSql, template.Parameters, cancellationToken: ct)
        );

        return new KeysetPaginated<AdminCategoryResponse, Guid>(
            items.ToList(),
            keysetPagination,
            response => response.CreatedAt,
            response => response.Id
        );
    }
}
