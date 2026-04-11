using Microsoft.Extensions.Logging;

namespace Infrastructure.Data.Shared;

internal sealed class UnitOfWork(AppDbContext dbContext, ILogger<UnitOfWork> logger) : IUnitOfWork
{
    public async Task<int> SaveChangesAsync(CancellationToken ct = default) => await dbContext.SaveChangesAsync(ct);

    public async Task BeginTransactionAsync(Func<Task> func, CancellationToken ct = default)
    {
        await using var tx = await dbContext.Database.BeginTransactionAsync(ct);

        try
        {
            await func();
            await tx.CommitAsync(ct);
        }
        catch (Exception e)
        {
            logger.LogError(e, "Transaction failed");
            await tx.RollbackAsync(ct);
            throw;
        }
    }
}