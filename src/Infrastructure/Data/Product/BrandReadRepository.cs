using System.Data;
using Application.Admin.Brands;
using Application.Admin.Brands.Types;
using Dapper;
using Domain.Shared.ValueObjects;
using Infrastructure.Data.Extensions;

namespace Infrastructure.Data.Product;

internal sealed class BrandReadRepository(IDbConnection connection) : IBrandReadRepository
{
    public async Task<KeysetPaginated<AdminBrandResponse, Guid>> GetBrandsAsync(
        NonEmptyString? search,
        KeysetPagination<Guid> keysetPagination,
        CancellationToken ct = default
    )
    {
        var builder = new SqlBuilder();
        var searchTerm = search?.Value.ToLower();

        builder.WhereNotNull(search, "b.name ILIKE @Search", new { Search = $"%{searchTerm}%" });
        builder.ApplyKeysetPagination(keysetPagination, "b");

        var template = builder.AddTemplate(
            """
                SELECT id as Id,
                       created_at as CreatedAt,
                       updated_at as UpdatedAt,
                       name as Name
                FROM brand b
                /**where**/
                /**orderby**/
                LIMIT @LimitPlusOne;
            """,
            new { LimitPlusOne = keysetPagination.Limit + 1 }
        );

        var items = await connection.QueryAsync<AdminBrandResponse>(
            new CommandDefinition(template.RawSql, template.Parameters, cancellationToken: ct)
        );

        return new KeysetPaginated<AdminBrandResponse, Guid>(
            items.ToList(),
            keysetPagination,
            response => response.CreatedAt,
            response => response.Id
        );
    }
}
