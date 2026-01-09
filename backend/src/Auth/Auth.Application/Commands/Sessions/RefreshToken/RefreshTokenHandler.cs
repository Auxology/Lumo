using Auth.Application.Abstractions.Authentication;
using Auth.Application.Abstractions.Data;
using Auth.Domain.Aggregates;
using Auth.Domain.Constants;
using Auth.Domain.Faults;
using Auth.Domain.ValueObjects;

using Microsoft.EntityFrameworkCore;

using SharedKernel;
using SharedKernel.Application.Authentication;
using SharedKernel.Application.Messaging;

namespace Auth.Application.Commands.Sessions.RefreshToken;

internal sealed class RefreshTokenHandler(
    IAuthDbContext dbContext,
    IRequestContext requestContext,
    ISecureTokenGenerator secureTokenGenerator,
    ITokenProvider tokenProvider,
    IDateTimeProvider dateTimeProvider) : ICommandHandler<RefreshTokenCommand, RefreshTokenResponse>
{
    public async ValueTask<Outcome<RefreshTokenResponse>> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
    {
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

        string[] parts = request.RefreshToken.Split('.');

        if (parts.Length != 2)
            return SessionFaults.RefreshTokenInvalidOrExpired;

        string refreshTokenKey = parts[0];
        string refreshToken = parts[1];

        if (string.IsNullOrWhiteSpace(refreshTokenKey) || string.IsNullOrWhiteSpace(refreshToken))
            return SessionFaults.RefreshTokenInvalidOrExpired;

        Session? session = await dbContext.Sessions
            .FirstOrDefaultAsync(s => s.RefreshTokenKey == refreshTokenKey, cancellationToken);

        if (session is null)
            return SessionFaults.RefreshTokenInvalidOrExpired;

        bool isValid = secureTokenGenerator.VerifyToken(refreshToken, session.RefreshTokenHash);

        if (!isValid)
            return SessionFaults.RefreshTokenInvalidOrExpired;

        string newRefreshTokenKey = secureTokenGenerator.GenerateToken(SessionConstants.RefreshTokenKeyLength);
        string newRefreshToken = secureTokenGenerator.GenerateToken(SessionConstants.RefreshTokenLength);
        string newRefreshTokenHash = secureTokenGenerator.HashToken(newRefreshToken);

        Outcome refreshOutcome = session.Refresh
        (
            newRefreshTokenKey: newRefreshTokenKey,
            newRefreshTokenHash: newRefreshTokenHash,
            fingerprint: fingerprint,
            dateTimeProvider.UtcNow
        );

        if (refreshOutcome.IsFailure)
            return SessionFaults.RefreshTokenInvalidOrExpired;

        try
        {
            await dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException)
        {
            return SessionFaults.RefreshTokenInvalidOrExpired;
        }

        string accessToken = tokenProvider.CreateToken(session.UserId.Value, session.Id.Value);

        RefreshTokenResponse refreshTokenResponse = new
        (
            AccessToken: accessToken,
            RefreshToken: $"{newRefreshTokenKey}.{newRefreshToken}"
        );

        return refreshTokenResponse;
    }
}