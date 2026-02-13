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

namespace Auth.Application.Commands.RecoveryRequests.Initiate;

internal sealed class InitiateRecoveryHandler(
    IAuthDbContext dbContext,
    IRequestContext requestContext,
    ISecureTokenGenerator secureTokenGenerator,
    IIdGenerator idGenerator,
    IMessageBus messageBus,
    IDateTimeProvider dateTimeProvider) : ICommandHandler<InitiateRecoveryCommand, InitiateRecoveryResponse>
{
    public async ValueTask<Outcome<InitiateRecoveryResponse>> Handle(InitiateRecoveryCommand request,
        CancellationToken cancellationToken)
    {
        (string identifier, string verifier)? parsed = ParseRecoveryKey(request.RecoveryKey);

        if (parsed is null)
            return RecoveryKeyOperationFaults.Invalid;

        (string identifier, string verifier) = parsed.Value;

        Outcome<EmailAddress> newEmailAddressOutcome = EmailAddress.Create(request.NewEmailAddress);

        if (newEmailAddressOutcome.IsFailure)
            return newEmailAddressOutcome.Fault;

        EmailAddress newEmailAddress = newEmailAddressOutcome.Value;

        bool emailExists = await dbContext.Users
            .AnyAsync(u => u.EmailAddress == newEmailAddress, cancellationToken);

        if (emailExists)
            return UserOperationFaults.EmailAlreadyInUse;

        RecoveryKeyChain? recoveryKeyChain = await dbContext.RecoveryKeyChains
            .Include(rkc => rkc.RecoveryKeys)
            .Where(rkc => rkc.RecoveryKeys.Any(rk => rk.Identifier == identifier && !rk.IsUsed))
            .FirstOrDefaultAsync(cancellationToken);

        if (recoveryKeyChain is null)
            return RecoveryKeyOperationFaults.Invalid;

        string? storedHash = recoveryKeyChain.GetVerifierHashForKey(identifier);

        if (storedHash is null)
            return RecoveryKeyOperationFaults.Invalid;

        bool isVerifierValid = secureTokenGenerator.VerifyToken
        (
            token: verifier,
            hashedToken: storedHash
        );

        if (!isVerifierValid)
            return RecoveryKeyOperationFaults.Invalid;

        User? user = await dbContext.Users
            .Where(u => u.Id == recoveryKeyChain.UserId)
            .FirstOrDefaultAsync(cancellationToken);

        if (user is null)
            return RecoveryKeyOperationFaults.Invalid;

        if (newEmailAddress == user.EmailAddress)
            return RecoveryKeyOperationFaults.Invalid;

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

        Fingerprint fingerprint = fingerprintOutcome.Value;

        Outcome markKeyAsUsedOutcome = recoveryKeyChain.MarkKeyAsUsed
        (
            identifier: identifier,
            fingerprint: fingerprint,
            utcNow: dateTimeProvider.UtcNow
        );

        if (markKeyAsUsedOutcome.IsFailure)
            return markKeyAsUsedOutcome.Fault;

        string tokenKey = secureTokenGenerator.GenerateToken(RecoveryRequestConstants.TokenKeyLength);
        string otpToken = secureTokenGenerator.GenerateToken(RecoveryRequestConstants.OtpTokenLength);
        string magicLinkToken = secureTokenGenerator.GenerateToken(RecoveryRequestConstants.MagicLinkTokenLength);
        string otpTokenHash = secureTokenGenerator.HashToken(otpToken);
        string magicLinkTokenHash = secureTokenGenerator.HashToken(magicLinkToken);

        RecoveryRequestId recoveryRequestId = idGenerator.NewRecoveryRequestId();

        Outcome<RecoveryRequest> recoveryRequestOutcome = RecoveryRequest.Create
        (
            id: recoveryRequestId,
            userId: user.Id,
            tokenKey: tokenKey,
            newEmailAddress: newEmailAddress,
            otpTokenHash: otpTokenHash,
            magicLinkTokenHash: magicLinkTokenHash,
            fingerprint: fingerprint,
            utcNow: dateTimeProvider.UtcNow
        );

        if (recoveryRequestOutcome.IsFailure)
            return recoveryRequestOutcome.Fault;

        RecoveryRequest recoveryRequest = recoveryRequestOutcome.Value;

        RecoveryInitiated recoveryInitiated = new()
        {
            EventId = Guid.NewGuid(),
            OccurredAt = dateTimeProvider.UtcNow,
            CorrelationId = Guid.Parse(requestContext.CorrelationId),
            UserId = user.Id.Value,
            OldEmailAddress = user.EmailAddress.Value,
            NewEmailAddress = newEmailAddress.Value,
            OtpToken = otpToken,
            MagicLinkToken = magicLinkToken,
            ExpiresAt = recoveryRequest.ExpiresAt,
            IpAddress = requestContext.IpAddress,
            UserAgent = requestContext.UserAgent
        };

        await dbContext.RecoveryRequests.AddAsync(recoveryRequest, cancellationToken);
        await messageBus.PublishAsync(recoveryInitiated, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);

        int remainingKeys = recoveryKeyChain.GetRemainingKeyCount();

        InitiateRecoveryResponse response = new
        (
            TokenKey: tokenKey,
            RemainingRecoveryKeys: remainingKeys
        );

        return response;
    }

    private static (string Identifier, string Verifier)? ParseRecoveryKey(string recoveryKey)
    {
        int dotIndex = recoveryKey.IndexOf('.', StringComparison.Ordinal);

        if (dotIndex <= 0 || dotIndex >= recoveryKey.Length - 1)
            return null;

        string identifier = recoveryKey[..dotIndex];
        string verifier = recoveryKey[(dotIndex + 1)..];
        return (identifier, verifier);
    }
}