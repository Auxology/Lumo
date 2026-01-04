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

        User newUser = new()
        {
            UserId = context.Message.UserId,
            DisplayName = context.Message.DisplayName,
            EmailAddress = context.Message.EmailAddress,
        };

        await dbContext.Users.AddAsync(newUser, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);

        logger.LogInformation("User read model created. UserId: {UserId}, EventId: {EventId}",
            context.Message.UserId, context.Message.EventId);
    }
}