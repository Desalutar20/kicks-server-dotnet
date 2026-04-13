using System.Linq.Expressions;

namespace Infrastructure.Data.User;

internal sealed class UserRepository(AppDbContext dbContext)
    : RepositoryBase<DomainUser>(dbContext), IUserRepository
{
    public async Task<DomainUser?>
        GetUserByEmailAsync(Email email, bool trackChanges, CancellationToken ct = default) =>
        await FindByCondition(u => u.Email == email, trackChanges)
            .SingleOrDefaultAsync(ct);

    public async Task<DomainUser?> GetUserByIdAsync(UserId id, bool trackChanges, CancellationToken ct = default) =>
        await FindByCondition(u => u.Id == id, trackChanges)
            .FirstOrDefaultAsync(ct);

    public async Task<DomainUser?> GetUserByProviderIdAsync(OAuthProvider provider, ProviderId id, bool trackChanges,
        CancellationToken ct = default)
    {
        Expression<Func<DomainUser, bool>> predicate = provider switch
        {
            OAuthProvider.Google => u => u.GoogleId == id,
            OAuthProvider.Facebook => u => u.FacebookId == id,
            _ => throw new ArgumentException("Provider not supported")
        };

        return await FindByCondition(predicate, trackChanges)
            .FirstOrDefaultAsync(ct);
    }

    public void CreateUser(DomainUser user)
    {
        Create(user);
    }

    public void UpdateUser(DomainUser user)
    {
        Update(user);
    }
}