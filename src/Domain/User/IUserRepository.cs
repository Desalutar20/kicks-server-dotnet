namespace Domain.User;

public interface IUserRepository
{
    Task<User?> GetUserByEmailAsync(Email email, bool trackChanges, CancellationToken ct = default);
    Task<User?> GetUserByIdAsync(UserId id, bool trackChanges, CancellationToken ct = default);

    Task<User?> GetUserByProviderIdAsync(OAuthProvider provider, ProviderId id, bool trackChanges,
        CancellationToken ct = default);

    void CreateUser(User user);
    void UpdateUser(User user);
}