namespace Domain.Abstractions.Events;

public interface IDomainEventHandler<in T>
    where T : IDomainEvent
{
    Task Handle(T @event, CancellationToken ct = default);
}
