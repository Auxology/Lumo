using Contracts.IntegrationEvents.Auth;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Notifications.Api.Data;
using Notifications.Api.Entities;
using SharedKernel;

namespace Notifications.Api.Consumers;

internal sealed class LoginRequestedConsumer(
    INotificationDbContext notificationDbContext,
    IDateTimeProvider dateTimeProvider,
    ILogger<LoginRequestedConsumer> logger) : IConsumer<LoginRequested>
{
    public async Task Consume(ConsumeContext<LoginRequested> context)
    {
        CancellationToken cancellationToken = context.CancellationToken;
        Guid eventId = context.Message.EventId;

        bool alreadyProcessed = await notificationDbContext.ProcessedEvents
            .AnyAsync(e => e.EventId == eventId, cancellationToken);

        if (alreadyProcessed)
            return;

        ProcessedEvent processedEvent = ProcessedEvent.Create(eventId, dateTimeProvider.UtcNow);

        try
        {
            await notificationDbContext.ProcessedEvents.AddAsync(processedEvent, cancellationToken);
            await notificationDbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException)
        {
            return;
        }
    }
}
