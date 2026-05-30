using Application.Abstractions.Outbox;

namespace Infrastructure.Data.Outbox;

internal sealed class OutboxRepository(AppDbContext dbContext)
    : RepositoryBase<Application.Abstractions.Outbox.Outbox>(dbContext),
        IOutboxRepository
{
    private readonly AppDbContext _dbContext = dbContext;

    public void CreateOutbox(Application.Abstractions.Outbox.Outbox outbox)
    {
        Create(outbox);
    }

    public async Task<
        IEnumerable<Application.Abstractions.Outbox.Outbox>
    > GetAndLockOutboxesForProcessingAsync(
        OutboxType type,
        PositiveInt batchSize,
        CancellationToken ct = default
    )
    {
        return await _dbContext
            .Outboxes.FromSqlInterpolated(
                $"""
                    SELECT *
                    FROM outbox
                    WHERE processed_at IS NULL
                      AND type = {type.ToString().ToLower()}
                    ORDER BY created_at
                    FOR UPDATE SKIP LOCKED
                    LIMIT {batchSize.Value}
                """
            )
            .AsNoTracking()
            .ToListAsync(ct);
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
