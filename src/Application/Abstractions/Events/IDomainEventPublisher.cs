namespace Application.Abstractions.Events;

public interface IDomainEventPublisher
{
    Task Publish(IEnumerable<IDomainEvent> events, CancellationToken ct = default);
}
