using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Api.Extensions;

public static class AppExtensions
{
    public static async Task ApplyMigrations(this WebApplication app)
    {
        await using var scope = app.Services.CreateAsyncScope();

        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        await dbContext.Database.MigrateAsync();
    }
}
