using MediatR;
using SharedKernel.Application.Messaging;
using SharedKernel.Domain;

namespace SharedKernel.Infrastructure.DomainEvents;

public sealed class DomainEventsDispatcher(IPublisher publisher) : IDomainEventsDispatcher
{
    public async Task DispatchAsync(IDomainEvent domainEvent, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(domainEvent);

        INotification notification = ToNotification(domainEvent);

        await publisher.Publish(notification, cancellationToken);
    }

    public async Task DispatchAllAsync(IEnumerable<IDomainEvent> domainEvents, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(domainEvents);

        await Task.WhenAll(domainEvents.Select(de => DispatchAsync(de, cancellationToken)));
    }

    private static INotification ToNotification(IDomainEvent domainEvent)
    {
        Type notificationType = typeof(DomainEventNotification<>).MakeGenericType(domainEvent.GetType());

        return (INotification)Activator.CreateInstance(notificationType, domainEvent)!;
    }
}
