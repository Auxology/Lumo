using Contracts.IntegrationEvents.Auth;

using Main.Application.Abstractions.Data;
using Main.Domain.ReadModels;

using MassTransit;

using Microsoft.Extensions.Logging;

namespace Main.Infrastructure.Consumers;

internal sealed class UserSignedUpConsumer(
    IMainDbContext dbContext,
    ILogger<UserSignedUpConsumer> logger) : IConsumer<UserSignedUp>
{
    public async Task Consume(ConsumeContext<UserSignedUp> context)
    {
        CancellationToken cancellationToken = context.CancellationToken;
        UserSignedUp message = context.Message;

        User newUser = new()
        {
            UserId = message.UserId,
            DisplayName = message.DisplayName,
            EmailAddress = message.EmailAddress,
        };

        await dbContext.Users.AddAsync(newUser, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);

        logger.LogInformation(
            "Consumed {EventType}: {EventId}, CorrelationId: {CorrelationId}, OccurredAt: {OccurredAt}, UserId: {UserId}",
            nameof(UserSignedUp), message.EventId, message.CorrelationId, message.OccurredAt, message.UserId);
    }
}