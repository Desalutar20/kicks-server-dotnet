using Domain.Outbox;

namespace Infrastructure.Data.Outbox;

internal sealed class OutboxRepository(AppDbContext dbContext)
    : RepositoryBase<DomainOutbox>(dbContext),
        IOutboxRepository
{
    private readonly AppDbContext _dbContext = dbContext;

    public void CreateOutbox(DomainOutbox outbox)
    {
        Create(outbox);
    }

    public async Task<IEnumerable<DomainOutbox>> GetAndLockOutboxesForProcessingAsync(
        OutboxType type,
        PositiveInt batchSize,
        bool trackChanges,
        CancellationToken ct = default
    )
    {
        var query = _dbContext.Outboxes.FromSqlInterpolated(
            $"""
                SELECT * FROM outbox
                WHERE processed_at IS NULL
                  AND type = {type.ToString().ToLower()}
                FOR UPDATE SKIP LOCKED
            """
        );

        if (!trackChanges)
            query = query.AsNoTracking();

        var result = await query.OrderBy(x => x.CreatedAt).Take(batchSize.Value).ToListAsync(ct);

        return result;
    }

    public async Task MarkAsProcessedAsync(
        IReadOnlyCollection<OutboxId> ids,
        DateTimeOffset now,
        CancellationToken ct = default
    )
    {
        await _dbContext
            .Outboxes.Where(x => ids.Contains(x.Id))
            .ExecuteUpdateAsync(
                s => s.SetProperty(x => x.ProcessedAt, now).SetProperty(x => x.UpdatedAt, now),
                ct
            );
    }
}
