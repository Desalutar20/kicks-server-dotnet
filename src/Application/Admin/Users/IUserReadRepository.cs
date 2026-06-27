using Application.Admin.Users.Types;

namespace Application.Admin.Users;

public interface IUserReadRepository
{
    Task<KeysetPaginated<AdminUserResponse, Guid>> GetUsersAsync(
        UsersFilters filters,
        KeysetPagination<Guid> keysetPagination,
        CancellationToken ct = default
    );
}
