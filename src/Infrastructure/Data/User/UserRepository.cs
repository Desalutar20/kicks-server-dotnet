using Infrastructure.Data.Extensions;

namespace Infrastructure.Data.User;

internal sealed class UserRepository(AppDbContext dbContext)
    : RepositoryBase<DomainUser>(dbContext),
        IUserRepository
{
    private readonly AppDbContext _dbContext = dbContext;

    public async Task<KeysetPaginated<DomainUser, UserId>> GetUsersAsync(
        UsersFilters filters,
        KeysetPagination<UserId> keysetPagination,
        bool trackChanges,
        CancellationToken ct = default
    )
    {
        var query = _dbContext.Users.AsQueryable();

        if (!trackChanges)
        {
            query = query.AsNoTracking();
        }

        var search = filters.Search?.Value.ToLower();

        query = query
            .WhereNotNull(
                search,
                u =>
                    ((string)u.Email).ToLower().StartsWith(search!)
                    || (u.FirstName != null && ((string)u.FirstName).ToLower().StartsWith(search!))
                    || (u.LastName != null && ((string)u.LastName).ToLower().StartsWith(search!))
            )
            .WhereNotNull(filters.Gender, u => u.Gender == filters.Gender)
            .WhereNotNull(filters.IsVerified, u => u.IsVerified == filters.IsVerified)
            .WhereNotNull(filters.IsBanned, u => u.IsBanned == filters.IsBanned)
            .ApplyKeysetPagination(keysetPagination);

        var result = await query.ToListAsync(ct);

        return new KeysetPaginated<DomainUser, UserId>(
            result,
            keysetPagination,
            u => u.CreatedAt,
            u => u.Id
        );
    }

    public async Task<DomainUser?> GetUserByIdAsync(
        UserId id,
        bool trackChanges,
        CancellationToken ct = default
    ) => await FindByCondition(u => u.Id == id, trackChanges).FirstOrDefaultAsync(ct);

    public async Task<DomainUser?> GetUserByEmailAsync(
        Email email,
        bool trackChanges,
        CancellationToken ct = default
    ) => await FindByCondition(u => u.Email == email, trackChanges).FirstOrDefaultAsync(ct);

    public void CreateUser(DomainUser user)
    {
        Create(user);
    }

    public void UpdateUser(DomainUser user)
    {
        Update(user);
    }

    public void DeleteUser(DomainUser user)
    {
        Delete(user);
    }
}
