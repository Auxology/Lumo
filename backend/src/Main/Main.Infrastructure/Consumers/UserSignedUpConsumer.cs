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
    IDateTimeProvider dateTimeProvider,
    ILogger<UserSignedUpConsumer> logger) : IConsumer<UserSignedUp>
{
    public async Task Consume(ConsumeContext<UserSignedUp> context)
    {
        CancellationToken cancellationToken = context.CancellationToken;
        Guid eventId = context.Message.EventId;

        bool alreadyProcessed = await dbContext.ProcessedEvents
            .AnyAsync(e => e.EventId == eventId, cancellationToken);

        if (alreadyProcessed)
            return;

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
            await dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException)
        {
            return;
        }

        logger.LogInformation("User read model created. UserId: {UserId}, EventId: {EventId}", context.Message.UserId,
            eventId);
    }
}
