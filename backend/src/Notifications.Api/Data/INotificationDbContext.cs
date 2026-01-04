using Microsoft.EntityFrameworkCore.Infrastructure;

namespace Notifications.Api.Data;

internal interface INotificationDbContext
{
    DatabaseFacade Database { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}