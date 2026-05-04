using Application.Admin.Users.Types;
using Domain.Shared.Pagination;

namespace Application.Admin.Users.UseCases.GetAllAdminUsers;

public sealed record GetAllAdminUsersQuery(
    UsersFilters Filters,
    KeysetPagination<UserId> KeysetPagination
) : IQuery<KeysetPaginated<AdminUser, UserId>>;

internal sealed class GetAllAdminUsersQueryHandler(IUserRepository userRepository)
    : IQueryHandler<GetAllAdminUsersQuery, KeysetPaginated<AdminUser, UserId>>
{
    public async Task<Result<KeysetPaginated<AdminUser, UserId>>> Handle(
        GetAllAdminUsersQuery query,
        CancellationToken ct = default
    )
    {
        var data = await userRepository.GetUsersAsync(
            query.Filters,
            query.KeysetPagination,
            false,
            ct
        );

        return Result<KeysetPaginated<AdminUser, UserId>>.Success(
            new KeysetPaginated<AdminUser, UserId>(
                [.. data.Data.Select(u => u.ToAdminUser())],
                data.PrevCursor,
                data.NextCursor
            )
        );
    }
}
