using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Notifications.Api.Entities;

namespace Notifications.Api.Data;

internal interface INotificationDbContext
{
    DbSet<ProcessedEvent> ProcessedEvents { get; }

    DatabaseFacade Database { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
