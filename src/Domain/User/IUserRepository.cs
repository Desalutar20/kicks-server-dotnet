using Domain.Shared.Pagination;

namespace Domain.User;

public interface IUserRepository
{
    Task<KeysetPaginated<User, UserId>> GetUsersAsync(
        UsersFilters filters,
        KeysetPagination<UserId> keysetPagination,
        bool trackChanges,
        CancellationToken ct = default
    );

    Task<User?> GetUserByEmailAsync(Email email, bool trackChanges, CancellationToken ct = default);
    Task<User?> GetUserByIdAsync(UserId id, bool trackChanges, CancellationToken ct = default);

    void CreateUser(User user);
    void UpdateUser(User user);
    void DeleteUser(User user);
}
