using Microsoft.EntityFrameworkCore;
using Notifications.Api.Entities;

namespace Notifications.Api.Data;

internal interface INotificationDbContext
{
    DbSet<ProcessedEvent> ProcessedEvents { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
