using Application.Abstractions.Database;
using Microsoft.EntityFrameworkCore.Storage;

namespace Infrastructure.Data;

internal sealed class EfTransaction(IDbContextTransaction transaction) : IDbTransaction
{
    public Task CommitAsync(CancellationToken ct = default) => transaction.CommitAsync(ct);

    public async ValueTask DisposeAsync() => await transaction.DisposeAsync();

    public Task RollbackAsync(CancellationToken ct = default) => transaction.RollbackAsync(ct);
}
