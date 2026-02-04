using Auth.Application.Abstractions.Data;
using Auth.Application.Faults;
using Auth.Domain.Aggregates;
using Auth.Domain.ValueObjects;

using Contracts.IntegrationEvents.Auth;

using Microsoft.EntityFrameworkCore;

using SharedKernel;
using SharedKernel.Application.Authentication;
using SharedKernel.Application.Messaging;

namespace Auth.Application.Commands.Users.CancelDeletion;

internal sealed class CancelUserDeletionHandler(
    IAuthDbContext dbContext,
    IUserContext userContext,
    IMessageBus messageBus,
    IDateTimeProvider dateTimeProvider) : ICommandHandler<CancelUserDeletionCommand, CancelUserDeletionResponse>
{
    public async ValueTask<Outcome<CancelUserDeletionResponse>> Handle(CancelUserDeletionCommand request, CancellationToken cancellationToken)
    {
        Outcome<UserId> userIdOutcome = UserId.FromGuid(userContext.UserId);

        if (userIdOutcome.IsFailure)
            return userIdOutcome.Fault;

        UserId userId = userIdOutcome.Value;

        User? user = await dbContext.Users
            .FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);

        if (user is null)
            return UserOperationFaults.NotFound;

        Outcome cancelDeletionOutcome = user.CancelDeletion(dateTimeProvider.UtcNow);

        if (cancelDeletionOutcome.IsFailure)
            return cancelDeletionOutcome.Fault;

        UserDeletionCanceled userDeletionCanceled = new()
        {
            EventId = Guid.NewGuid(),
            OccurredAt = dateTimeProvider.UtcNow,
            CorrelationId = Guid.NewGuid(),
            UserId = user.Id.Value,
            EmailAddress = user.EmailAddress.Value
        };

        await messageBus.PublishAsync(userDeletionCanceled, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);

        CancelUserDeletionResponse response = new
        (
            Id: user.Id.Value,
            CanceledAt: userDeletionCanceled.OccurredAt
        );

        return response;
    }
}