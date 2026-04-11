namespace Domain.User;

public interface IUserRepository
{
    Task<User?> GetByEmailAsync(Email email, bool trackChanges, CancellationToken ct = default);
    Task<User?> GetByIdAsync(UserId id, bool trackChanges, CancellationToken ct = default);

    void CreateUser(User user);
    void UpdateUser(User user);
}