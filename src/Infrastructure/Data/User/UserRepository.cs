namespace Infrastructure.Data.User;

internal sealed class UserRepository(AppDbContext dbContext)
    : RepositoryBase<DomainUser>(dbContext), IUserRepository
{
    public async Task<DomainUser?> GetByEmailAsync(Email email, bool trackChanges, CancellationToken ct = default) =>
        await FindByCondition(u => u.Email == email, trackChanges).SingleOrDefaultAsync(ct);

    public async Task<DomainUser?> GetByIdAsync(UserId id, bool trackChanges, CancellationToken ct = default) =>
        await FindByCondition(u => u.Id == id, trackChanges).FirstOrDefaultAsync(ct);

    public void CreateUser(DomainUser user)
    {
        Create(user);
    }

    public void UpdateUser(DomainUser user)
    {
        Update(user);
    }
}