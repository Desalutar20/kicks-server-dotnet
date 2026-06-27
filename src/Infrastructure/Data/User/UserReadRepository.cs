using System.Data;
using Application.Admin.Users;
using Application.Admin.Users.Types;
using Dapper;
using Infrastructure.Data.Extensions;

namespace Infrastructure.Data.User;

internal sealed class UserReadRepository(IDbConnection connection) : IUserReadRepository
{
    public async Task<KeysetPaginated<AdminUserResponse, Guid>> GetUsersAsync(
        UsersFilters filters,
        KeysetPagination<Guid> keysetPagination,
        CancellationToken ct = default
    )
    {
        var builder = new SqlBuilder();

        var search = filters.Search?.Value.ToLower();

        builder.WhereNotNull(
            search,
            "(u.email ILIKE @Search OR u.first_name ILIKE @Search OR u.last_name ILIKE @Search)",
            new { Search = $"{search}%" }
        );

        builder.WhereNotNull(
            filters.Gender,
            "u.gender = @Gender",
            new { Gender = filters.Gender.ToString()?.ToLower() }
        );

        builder.WhereNotNull(
            filters.IsVerified,
            "u.is_verified = @IsVerified",
            new { filters.IsVerified }
        );

        builder.WhereNotNull(filters.IsBanned, "u.is_banned = @IsBanned", new { filters.IsBanned });

        builder.ApplyKeysetPagination(keysetPagination, "u");

        var template = builder.AddTemplate(
            """
                SELECT id as Id,
                       created_at as CreatedAt,
                       updated_at as UpdatedAt,
                       email as Email,
                       first_name as FirstName,
                       last_name as LastName,
                       role as Role,
                       gender as Gender,
                       is_verified as IsVerified,
                       is_banned as IsBanned,
                       google_id as GoogleId,
                       facebook_id as FacebookId
                FROM users u
                /**where**/
                /**orderby**/
                LIMIT @LimitPlusOne;
            """,
            new { LimitPlusOne = keysetPagination.Limit + 1 }
        );

        var items = await connection.QueryAsync<AdminUserResponse>(
            new CommandDefinition(template.RawSql, template.Parameters, cancellationToken: ct)
        );

        return new KeysetPaginated<AdminUserResponse, Guid>(
            items.ToList(),
            keysetPagination,
            response => response.CreatedAt,
            response => response.Id
        );
    }
}
