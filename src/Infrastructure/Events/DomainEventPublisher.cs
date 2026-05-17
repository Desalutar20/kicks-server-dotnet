using Application.Abstractions.Events;

namespace Infrastructure.Events;

internal sealed class DomainEventPublisher(IServiceProvider serviceProvider) : IDomainEventPublisher
{
    public async Task Publish(IEnumerable<IDomainEvent> events, CancellationToken ct = default)
    {
        using var scope = serviceProvider.CreateScope();
        var scopedProvider = scope.ServiceProvider;

        foreach (var domainEvent in events)
        {
            var handlerType = typeof(IDomainEventHandler<>).MakeGenericType(domainEvent.GetType());
            var handlers = scopedProvider.GetServices(handlerType);

            foreach (var handler in handlers)
            {
                var handleMethod = handlerType.GetMethod("Handle");
                if (handleMethod is null)
                    continue;

                await (Task)handleMethod.Invoke(handler, new object[] { domainEvent, ct })!;
            }
        }
    }
}
