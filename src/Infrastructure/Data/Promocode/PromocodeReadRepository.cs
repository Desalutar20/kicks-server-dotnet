using System.Data;
using Application.Admin.Promocodes;
using Application.Admin.Promocodes.Types;
using Dapper;
using Domain.Promocodes;
using Infrastructure.Data.Extensions;

namespace Infrastructure.Data.Promocode;

internal sealed class PromocodeReadRepository(IDbConnection connection) : IPromocodeReadRepository
{
    public async Task<KeysetPaginated<AdminPromocodeResponse, Guid>> GetPromocodesAsync(
        PromocodeCode? code,
        KeysetPagination<Guid> keysetPagination,
        CancellationToken ct = default
    )
    {
        var builder = new SqlBuilder();

        builder.WhereNotNull(code, "p.code = @Code", new { Code = code });
        builder.ApplyKeysetPagination(keysetPagination, "p");

        var template = builder.AddTemplate(
            """
                SELECT id as Id,
                       created_at as CreatedAt,
                       updated_at as UpdatedAt,
                       discount_value as DiscountValue,
                       valid_from as ValidFrom,
                       valid_to as ValidTo,
                       usage_limit as UsageLimit,
                       usage_count as UsageCount,
                       code as Code
                FROM promocode p
                /**where**/
                /**orderby**/
                LIMIT @LimitPlusOne;
            """,
            new { LimitPlusOne = keysetPagination.Limit + 1 }
        );

        var items = await connection.QueryAsync<AdminPromocodeResponse>(
            new CommandDefinition(template.RawSql, template.Parameters, cancellationToken: ct)
        );

        return new KeysetPaginated<AdminPromocodeResponse, Guid>(
            items.ToList(),
            keysetPagination,
            response => response.CreatedAt,
            response => response.Id
        );
    }
}
