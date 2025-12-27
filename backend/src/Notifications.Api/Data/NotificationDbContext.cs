using MassTransit;
using Microsoft.EntityFrameworkCore;
using Notifications.Api.Entities;

namespace Notifications.Api.Data;

internal sealed class NotificationDbContext(DbContextOptions<NotificationDbContext> options)
    : DbContext(options), INotificationDbContext
{
    public DbSet<ProcessedEvent> ProcessedEvents { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(NotificationDbContext).Assembly);

        modelBuilder.AddInboxStateEntity();
        modelBuilder.AddOutboxStateEntity();
        modelBuilder.AddOutboxMessageEntity();
    }
}
