using Auth.Application.Abstractions.Authentication;
using Auth.Application.Abstractions.Data;
using Auth.Application.Abstractions.Generators;
using Auth.Application.Faults;
using Auth.Domain.Aggregates;
using Auth.Domain.Constants;
using Auth.Domain.ValueObjects;

using Contracts.IntegrationEvents.Auth;

using Microsoft.EntityFrameworkCore;

using SharedKernel;
using SharedKernel.Application.Authentication;
using SharedKernel.Application.Messaging;

namespace Auth.Application.Commands.EmailChangeRequests.Request;

internal sealed class RequestEmailChangeHandler(
    IAuthDbContext dbContext,
    IUserContext userContext,
    IRequestContext requestContext,
    ISecureTokenGenerator secureTokenGenerator,
    IIdGenerator idGenerator,
    IMessageBus messageBus,
    IDateTimeProvider dateTimeProvider
) : ICommandHandler<RequestEmailChangeCommand, RequestEmailChangeResponse>
{
    public async ValueTask<Outcome<RequestEmailChangeResponse>> Handle(RequestEmailChangeCommand request,
        CancellationToken cancellationToken)

    {
        Outcome<UserId> userIdOutcome = UserId.FromGuid(userContext.UserId);

        if (userIdOutcome.IsFailure)
            return userIdOutcome.Fault;

        UserId userId = userIdOutcome.Value;

        Outcome<EmailAddress> newEmailAddressOutcome = EmailAddress.Create(request.NewEmailAddress);

        if (newEmailAddressOutcome.IsFailure)
            return newEmailAddressOutcome.Fault;

        EmailAddress newEmailAddress = newEmailAddressOutcome.Value;

        User? user = await dbContext.Users
            .FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);

        if (user is null)
            return UserOperationFaults.NotFound;

        bool emailInUse = await dbContext.Users
            .AnyAsync(u => u.EmailAddress == newEmailAddress, cancellationToken);

        if (emailInUse)
            return UserOperationFaults.EmailAlreadyInUse;

        List<EmailChangeRequest> pendingRequests = await dbContext.EmailChangeRequests
            .Where(ecr => ecr.UserId == userId && ecr.IsActive(dateTimeProvider.UtcNow))
            .ToListAsync(cancellationToken);

        foreach (EmailChangeRequest pendingRequest in pendingRequests)
            pendingRequest.Cancel(dateTimeProvider.UtcNow);

        Outcome<Fingerprint> fingerprintOutcome = Fingerprint.Create
        (
            ipAddress: requestContext.IpAddress,
            userAgent: requestContext.UserAgent,
            timezone: requestContext.Timezone,
            language: requestContext.Language,
            normalizedBrowser: requestContext.NormalizedBrowser,
            normalizedOs: requestContext.NormalizedOs
        );

        if (fingerprintOutcome.IsFailure)
            return fingerprintOutcome.Fault;

        string tokenKey = secureTokenGenerator.GenerateToken(EmailChangeRequestConstants.TokenKeyLength);
        string otpToken = secureTokenGenerator.GenerateToken(EmailChangeRequestConstants.OtpTokenLength);
        string otpTokenHash = secureTokenGenerator.HashToken(otpToken);

        EmailChangeRequestId emailChangeRequestId = idGenerator.NewEmailChangeRequestId();

        Outcome<EmailChangeRequest> emailChangeRequestOutcome = EmailChangeRequest.Create
        (
            id: emailChangeRequestId,
            userId: userId,
            tokenKey: tokenKey,
            currentEmailAddress: user.EmailAddress,
            newEmailAddress: newEmailAddress,
            otpTokenHash: otpTokenHash,
            fingerprint: fingerprintOutcome.Value,
            utcNow: dateTimeProvider.UtcNow
        );

        if (emailChangeRequestOutcome.IsFailure)
            return emailChangeRequestOutcome.Fault;

        EmailChangeRequest emailChangeRequest = emailChangeRequestOutcome.Value;

        EmailChangeRequested emailChangeRequested = new()
        {
            EventId = Guid.NewGuid(),
            OccurredAt = dateTimeProvider.UtcNow,
            CorrelationId = Guid.Parse(requestContext.CorrelationId),
            UserId = user.Id.Value,
            CurrentEmailAddress = user.EmailAddress.Value,
            NewEmailAddress = newEmailAddress.Value,
            OtpToken = otpToken,
            ExpiresAt = emailChangeRequest.ExpiresAt,
            IpAddress = requestContext.IpAddress,
            UserAgent = requestContext.UserAgent
        };

        await dbContext.EmailChangeRequests.AddAsync(emailChangeRequest, cancellationToken);
        await messageBus.PublishAsync(emailChangeRequested, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);

        RequestEmailChangeResponse response = new(tokenKey);

        return response;
    }
}