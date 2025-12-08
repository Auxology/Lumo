using MediatR;
using SharedKernel.Domain;

namespace SharedKernel.Infrastructure.DomainEvents;

public sealed class DomainEventsDispatcher(IPublisher publisher) : IDomainEventsDispatcher
{
    public async Task DispatchAsync(IDomainEvent domainEvent, CancellationToken cancellationToken = default)
    {
        await publisher.Publish(domainEvent, cancellationToken);
    }

    public async Task DispatchAllAsync(IEnumerable<IDomainEvent> domainEvents, CancellationToken cancellationToken = default)
    {
        await Task.WhenAll(domainEvents.Select(de => DispatchAsync(de, cancellationToken)));
    }
}