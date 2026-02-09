using Auth.Application.Abstractions.Data;
using Auth.Application.Faults;
using Auth.Domain.Aggregates;

using Contracts.IntegrationEvents.Auth;

using Microsoft.EntityFrameworkCore;

using SharedKernel;
using SharedKernel.Application.Authentication;
using SharedKernel.Application.Messaging;

namespace Auth.Application.Commands.RecoveryRequests.Complete;

internal sealed class CompleteRecoveryHandler(
    IAuthDbContext dbContext,
    IRequestContext requestContext,
    IMessageBus messageBus,
    IDateTimeProvider dateTimeProvider
) : ICommandHandler<CompleteRecoveryCommand, CompleteRecoveryResponse>
{
    public async ValueTask<Outcome<CompleteRecoveryResponse>> Handle(CompleteRecoveryCommand request,
        CancellationToken cancellationToken)
    {
        var data = await
        (
            from rr in dbContext.RecoveryRequests
            join u in dbContext.Users on rr.UserId equals u.Id
            where rr.TokenKey == request.TokenKey
            select new { RecoveryRequest = rr, User = u }
        ).FirstOrDefaultAsync(cancellationToken);

        if (data is null)
            return RecoveryRequestOperationFaults.InvalidOrExpired;

        RecoveryRequest recoveryRequest = data.RecoveryRequest;
        User user = data.User;

        Outcome completeOutcome = recoveryRequest.Complete(dateTimeProvider.UtcNow);

        if (completeOutcome.IsFailure)
            return completeOutcome.Fault;

        string oldEmailAddress = user.EmailAddress.Value;
        string newEmailAddress = recoveryRequest.NewEmailAddress.Value;

        Outcome emailChangeOutcome = user.ChangeEmailAddress
        (
            newEmailAddress: recoveryRequest.NewEmailAddress,
            dateTimeProvider.UtcNow
        );

        if (emailChangeOutcome.IsFailure)
            return emailChangeOutcome.Fault;

        List<Session> activeSessions = await dbContext.Sessions
            .Where(s => s.UserId == user.Id && s.RevokedAt == null)
            .ToListAsync(cancellationToken);

        foreach (Session session in activeSessions)
            session.RevokeDueToAccountRecovery(dateTimeProvider.UtcNow);

        UserEmailAddressChanged emailAddressChanged = new()
        {
            EventId = Guid.NewGuid(),
            OccurredAt = dateTimeProvider.UtcNow,
            CorrelationId = Guid.Parse(requestContext.CorrelationId),
            UserId = user.Id.Value,
            OldEmailAddress = oldEmailAddress,
            NewEmailAddress = newEmailAddress
        };

        await messageBus.PublishAsync(emailAddressChanged, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);

        CompleteRecoveryResponse recoveryResponse = new(newEmailAddress);

        return recoveryResponse;
    }
}