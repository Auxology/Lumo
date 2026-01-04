using Contracts.IntegrationEvents.Auth;
using Main.Application.Abstractions.Data;
using Main.Domain.Entities;
using Main.Domain.ReadModels;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SharedKernel;

namespace Main.Infrastructure.Consumers;

internal sealed class UserSignedUpConsumer(
    IMainDbContext dbContext,
    ILogger<UserSignedUpConsumer> logger,
    IDateTimeProvider dateTimeProvider) : IConsumer<UserSignedUp>
{
    public async Task Consume(ConsumeContext<UserSignedUp> context)
    {
        CancellationToken cancellationToken = context.CancellationToken;
        Guid eventId = context.Message.EventId;

        logger.LogInformation(
            "Received UserSignedUp event: EventId={EventId}, UserId={UserId}, Email={Email}, DisplayName={DisplayName}",
            eventId,
            context.Message.UserId,
            context.Message.EmailAddress,
            context.Message.DisplayName);

        bool alreadyProcessed = await dbContext.ProcessedEvents
            .AnyAsync(e => e.EventId == eventId, cancellationToken);

        if (alreadyProcessed)
        {
            logger.LogInformation("Event with ID {EventId} has already been processed. Skipping.", eventId);
            return;
        }

        ProcessedEvent processedEvent = ProcessedEvent.Create(eventId, dateTimeProvider.UtcNow);

        User newUser = new()
        {
            UserId = context.Message.UserId,
            DisplayName = context.Message.DisplayName,
            EmailAddress = context.Message.EmailAddress,
        };

        try
        {
            await dbContext.ProcessedEvents.AddAsync(processedEvent, cancellationToken);
            await dbContext.Users.AddAsync(newUser, cancellationToken);

            logger.LogInformation("About to save User {UserId} and ProcessedEvent {EventId}", newUser.UserId, eventId);

            int savedCount = await dbContext.SaveChangesAsync(cancellationToken);

            logger.LogInformation("SaveChangesAsync completed. Rows affected: {SavedCount}", savedCount);
        }
        catch (DbUpdateException ex)
        {
            logger.LogError(ex, "Database error while saving User {UserId} for event {EventId}", context.Message.UserId, eventId);
            throw;
        }

        logger.LogInformation("Successfully processed UserSignedUp event with ID {EventId}.", eventId);
    }
}
