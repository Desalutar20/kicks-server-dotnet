using Application.Abstractions.Database;
using Microsoft.EntityFrameworkCore.Storage;

namespace Infrastructure.Data;

internal sealed class EfTransaction(IDbContextTransaction transaction) : IDbTransaction
{
    public async Task CommitAsync(CancellationToken ct = default) =>
        await transaction.CommitAsync(ct);

    public async ValueTask DisposeAsync() => await transaction.DisposeAsync();

    public async Task RollbackAsync(CancellationToken ct = default) =>
        await transaction.RollbackAsync(ct);
}
