using Application.Admin.Users.Types;

namespace Application.Admin.Users.UseCases.GetAllAdminUsers;

public sealed record GetAllAdminUsersQuery(
    UsersFilters Filters,
    KeysetPagination<Guid> KeysetPagination
) : IQuery<KeysetPaginated<AdminUserResponse, Guid>>;

internal sealed class GetAllAdminUsersQueryHandler(IUserReadRepository userReadReadRepository)
    : IQueryHandler<GetAllAdminUsersQuery, KeysetPaginated<AdminUserResponse, Guid>>
{
    public async Task<Result<KeysetPaginated<AdminUserResponse, Guid>>> Handle(
        GetAllAdminUsersQuery query,
        CancellationToken ct = default
    )
    {
        var data = await userReadReadRepository.GetUsersAsync(
            query.Filters,
            query.KeysetPagination,
            ct
        );

        return data;
    }
}
