namespace Application.Abstractions.Database;

public interface IUnitOfWork
{
    Task<int> SaveChangesAsync(CancellationToken ct = default);
    Task<IDbTransaction> BeginTransactionAsync(CancellationToken ct = default);
}
