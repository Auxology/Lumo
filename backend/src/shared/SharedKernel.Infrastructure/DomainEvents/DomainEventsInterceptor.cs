using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using SharedKernel.Domain;

namespace SharedKernel.Infrastructure.DomainEvents;

public sealed class DomainEventsInterceptor(IDomainEventsDispatcher domainEventsDispatcher) : SaveChangesInterceptor
{
    public override async ValueTask<int> SavedChangesAsync
    (
        SaveChangesCompletedEventData eventData,
        int result,
        CancellationToken cancellationToken = default
    )
    {
        ArgumentNullException.ThrowIfNull(eventData);
        
        if (eventData.Context is not null)
        {
            await PublishDomainEventsAsync(eventData.Context, cancellationToken);
        }
        
        return result;
    }

    private async Task PublishDomainEventsAsync(DbContext context, CancellationToken cancellationToken)
    {
        var domainEvents = context.ChangeTracker
            .Entries<IHasDomainEvents>()
            .Select(x => x.Entity)
            .SelectMany(x =>
            {
                List<IDomainEvent> events = x.DomainEvents.ToList();
                
                x.ClearDomainEvents();
                
                return events;
            })
            .ToList();

        await domainEventsDispatcher.DispatchAllAsync(domainEvents, cancellationToken);
    }
}
