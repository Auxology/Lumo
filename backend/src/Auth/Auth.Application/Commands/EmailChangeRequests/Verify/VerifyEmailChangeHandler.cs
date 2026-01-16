using Auth.Application.Abstractions.Authentication;
using Auth.Application.Abstractions.Data;
using Auth.Application.Faults;
using Auth.Domain.Aggregates;
using Auth.Domain.ValueObjects;

using Contracts.IntegrationEvents.Auth;

using Microsoft.EntityFrameworkCore;

using SharedKernel;
using SharedKernel.Application.Authentication;
using SharedKernel.Application.Messaging;

namespace Auth.Application.Commands.EmailChangeRequests.Verify;

internal sealed class VerifyEmailChangeHandler(
    IAuthDbContext dbContext,
    IUserContext userContext,
    IRequestContext requestContext,
    ISecureTokenGenerator secureTokenGenerator,
    IMessageBus messageBus,
    IDateTimeProvider dateTimeProvider
) : ICommandHandler<VerifyEmailChangeCommand, VerifyEmailChangeResponse>
{
    public async ValueTask<Outcome<VerifyEmailChangeResponse>> Handle(VerifyEmailChangeCommand request,
        CancellationToken cancellationToken)

    {
        Outcome<UserId> userIdOutcome = UserId.FromGuid(userContext.UserId);

        if (userIdOutcome.IsFailure)
            return userIdOutcome.Fault;

        var data = await
        (
            from ecr in dbContext.EmailChangeRequests
            join u in dbContext.Users on ecr.UserId equals u.Id
            where ecr.TokenKey == request.TokenKey
            select new { EmailChangeRequest = ecr, User = u }
        ).FirstOrDefaultAsync(cancellationToken);

        if (data is null)
            return EmailChangeRequestOperationFaults.InvalidOrExpired;
        
        EmailChangeRequest emailChangeRequest = data.EmailChangeRequest;
        User user = data.User;
        
        if (!emailChangeRequest.IsActive(dateTimeProvider.UtcNow))
            return EmailChangeRequestOperationFaults.InvalidOrExpired;

        bool tokenValid = false;
        
        if (request.OtpToken is not null)
        {
            tokenValid = secureTokenGenerator.VerifyToken
            (
                token: request.OtpToken,
                hashedToken: emailChangeRequest.OtpTokenHash
            );
        }
        else if (request.MagicLinkToken is not null)
        {
            tokenValid = secureTokenGenerator.VerifyToken
            (
                token: request.MagicLinkToken,
                hashedToken: emailChangeRequest.MagicLinkTokenHash
            );
        }
        if (!tokenValid)
            return EmailChangeRequestOperationFaults.InvalidOrExpired;
        
        bool emailExists = await dbContext.Users
            .AnyAsync(u => u.EmailAddress == emailChangeRequest.NewEmailAddress, cancellationToken);

        if (emailExists)
            return UserOperationFaults.EmailAlreadyInUse;

        Outcome completeOutcome = emailChangeRequest.Complete(dateTimeProvider.UtcNow);
        
        if (completeOutcome.IsFailure)
            return completeOutcome.Fault;
        
        string oldEmailAddress = user.EmailAddress.Value;
        string newEmailAddress = emailChangeRequest.NewEmailAddress.Value;
        
        Outcome emailChangeOutcome = user.ChangeEmailAddress
        (
            newEmailAddress: emailChangeRequest.NewEmailAddress,
            utcNow: dateTimeProvider.UtcNow
        );

        if (emailChangeOutcome.IsFailure)
            return emailChangeOutcome.Fault;
        
        List<Session> activeSessions = await dbContext.Sessions
            .Where(s => s.UserId == user.Id && s.RevokedAt == null)
            .ToListAsync(cancellationToken);

        foreach (Session activeSession in activeSessions)
            activeSession.RevokeDueToEmailChange(dateTimeProvider.UtcNow);

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
        
        VerifyEmailChangeResponse response = new(newEmailAddress);
        
        return response;
    }
}