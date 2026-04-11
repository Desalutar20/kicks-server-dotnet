using Microsoft.EntityFrameworkCore;

namespace Integration.Setup;

public partial class TestApp
{
    protected async Task<User?> GetUserFromDbByEmail(Email email) =>
        await _dbContext.Users.AsNoTracking().SingleOrDefaultAsync(u => u.Email == email);

    protected async Task DeleteUserFromDbByEmail(Email email) =>
        await _dbContext.Users.Where(e => e.Email == email).ExecuteDeleteAsync();

    protected async Task BanUserInDbByEmail(Email email) =>
        await _dbContext.Users
                        .Where(u => u.Email == email)
                        .ExecuteUpdateAsync(setters =>
                            setters.SetProperty(u => u.IsBanned, true));
}