using Domain.Abstractions;
using Domain.Shared;

namespace Domain.Outbox;

public sealed class Outbox : Entity<OutboxId>
{
    private Outbox()
        : base(new OutboxId(Guid.NewGuid())) { }

    public OutboxType Type { get; private set; }
    public NonEmptyString Data { get; private set; }
    public DateTimeOffset? ProcessedAt { get; private set; }

    public static Outbox Create(OutboxType type, NonEmptyString data) =>
        new() { Type = type, Data = data };

    public void MarkAsProcessed()
    {
        ProcessedAt = DateTimeOffset.UtcNow;
    }
}
