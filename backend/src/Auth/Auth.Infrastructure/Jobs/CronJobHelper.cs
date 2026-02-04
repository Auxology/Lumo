using Auth.Application.Abstractions.Data;
using Auth.Domain.Aggregates;
using Auth.Domain.Constants;

using Contracts.IntegrationEvents.Auth;

using Microsoft.EntityFrameworkCore;

using SharedKernel;
using SharedKernel.Application.Messaging;

namespace Auth.Infrastructure.Jobs;

internal sealed class CronJobHelper(
    IAuthDbContext dbContext,
    IMessageBus messageBus,
    IDateTimeProvider dateTimeProvider) : ICronJobHelper
{
    public async Task HardDeleteUsersAsync(CancellationToken cancellationToken = default)
    {
        const int batchSize = 1000;
        DateTimeOffset cutOffTime = dateTimeProvider.UtcNow.AddDays(-UserConstants.AccountRecoveryPeriodInDays);

        while (true)
        {
            List<User> usersToDelete = await dbContext.Users
                .Where(user => user.DeletedAt.HasValue && user.DeletedAt <= cutOffTime)
                .Take(batchSize)
                .ToListAsync(cancellationToken);

            if (usersToDelete.Count == 0)
                return;

            dbContext.Users.RemoveRange(usersToDelete);

            foreach (User userToDelete in usersToDelete)
            {
                UserDeleted userDeleted = new()
                {
                    EventId = Guid.NewGuid(),
                    OccurredAt = dateTimeProvider.UtcNow,
                    CorrelationId = Guid.NewGuid(),
                    UserId = userToDelete.Id.Value,
                    EmailAddress = userToDelete.EmailAddress.Value,
                };

                await messageBus.PublishAsync(userDeleted, cancellationToken);
            }

            await dbContext.SaveChangesAsync(cancellationToken);
        }
    }
}