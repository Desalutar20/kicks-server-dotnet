using Domain.Shared.ValueObjects;

namespace Application.Abstractions.Outbox;

public interface IOutboxRepository
{
    void CreateOutbox(Outbox outbox);

    Task<IEnumerable<Outbox>> GetAndLockOutboxesForProcessingAsync(
        OutboxType type,
        PositiveInt batchSize,
        CancellationToken ct = default
    );

    Task MarkAsProcessedAsync(
        IReadOnlyCollection<OutboxId> ids,
        DateTimeOffset now,
        CancellationToken ct = default
    );
}
