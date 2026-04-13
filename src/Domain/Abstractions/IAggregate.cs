using Domain.Abstractions.Events;

namespace Domain.Abstractions;

public interface IAggregate
{
    IReadOnlyList<IDomainEvent> DomainEvents { get; }
    void ClearDomainEvents();
}