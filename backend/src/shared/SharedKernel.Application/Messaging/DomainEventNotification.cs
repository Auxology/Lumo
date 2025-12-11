using MediatR;
using SharedKernel.Domain;

namespace SharedKernel.Application.Messaging;

public sealed class DomainEventNotification<TEvent> : INotification
    where TEvent : IDomainEvent
{
    public TEvent DomainEvent { get; }

    public DomainEventNotification(TEvent domainEvent) => DomainEvent = domainEvent;
}
