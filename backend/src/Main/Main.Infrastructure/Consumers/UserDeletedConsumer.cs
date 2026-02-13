using Contracts.IntegrationEvents.Auth;

using Main.Application.Abstractions.Data;
using Main.Domain.ReadModels;

using MassTransit;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Main.Infrastructure.Consumers;

internal sealed class UserDeletedConsumer(IMainDbContext dbContext, ILogger<UserDeletedConsumer> logger)
    : IConsumer<UserDeleted>
{
    public async Task Consume(ConsumeContext<UserDeleted> context)
    {
        CancellationToken cancellationToken = context.CancellationToken;
        UserDeleted message = context.Message;

        User? user = await dbContext.Users
            .FirstOrDefaultAsync(u => u.UserId == message.UserId, cancellationToken);

        if (user is null)
        {
            logger.LogWarning(
                "User with ID {UserId} not found for deletion. EventId: {EventId}, CorrelationId: {CorrelationId}, OccurredAt: {OccurredAt}",
                message.UserId, message.EventId, message.CorrelationId, message.OccurredAt);
            return;
        }

        dbContext.Users.Remove(user);
        await dbContext.SaveChangesAsync(cancellationToken);

        if (logger.IsEnabled(LogLevel.Information))
            logger.LogInformation(
                "Consumed {EventType}: {EventId}, CorrelationId: {CorrelationId}, OccurredAt: {OccurredAt}, UserId: {UserId}",
                nameof(UserDeleted), message.EventId, message.CorrelationId, message.OccurredAt, message.UserId);
    }
}