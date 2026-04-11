using Domain.Shared;

namespace Domain.Outbox;

public interface IOutboxRepository
{
    void CreateOutbox(Outbox outbox);

    Task<IEnumerable<Outbox>> GetAndLockOutboxesForProcessingAsync(OutboxType type, PositiveInt batchSize,
        bool trackChanges,
        CancellationToken ct = default);

    Task MarkAsProcessedAsync(IReadOnlyCollection<OutboxId> ids, DateTimeOffset now, CancellationToken ct = default);
}