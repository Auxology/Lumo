using SharedKernel.Domain;

namespace SharedKernel.Infrastructure.DomainEvents;

public interface IDomainEventsDispatcher
{
    Task DispatchAsync(IDomainEvent domainEvent, CancellationToken cancellationToken = default);
    
    Task DispatchAllAsync(IEnumerable<IDomainEvent> domainEvents, CancellationToken cancellationToken = default);
}