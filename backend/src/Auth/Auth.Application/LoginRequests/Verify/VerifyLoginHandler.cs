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

namespace Auth.Application.LoginRequests.Verify;

internal sealed class VerifyLoginHandler(
    IAuthDbContext dbContext,
    IRequestContext requestContext,
    ISecureTokenGenerator secureTokenGenerator,
    ITokenProvider tokenProvider,
    IDateTimeProvider dateTimeProvider) : ICommandHandler<VerifyLoginCommand, VerifyLoginResponse>
{
    public async ValueTask<Outcome<VerifyLoginResponse>> Handle(VerifyLoginCommand request,
        CancellationToken cancellationToken)
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

        LoginRequest? loginRequest = await dbContext.LoginRequests
            .FirstOrDefaultAsync(lr => lr.TokenKey == request.TokenKey, cancellationToken);

        if (loginRequest is null)
            return LoginRequestFaults.InvalidOrExpired;

        bool isOtpValid = request.OtpToken is not null &&
                          secureTokenGenerator.VerifyToken(request.OtpToken, loginRequest.OtpTokenHash);

        bool isMagicLinkValid = request.MagicLinkToken is not null &&
                                secureTokenGenerator.VerifyToken(request.MagicLinkToken,
                                    loginRequest.MagicLinkTokenHash);

        bool isValid = isOtpValid || isMagicLinkValid;

        if (!isValid)
            return LoginRequestFaults.InvalidOrExpired;

        Outcome consumeOutcome = loginRequest.Consume(dateTimeProvider.UtcNow);

        if (consumeOutcome.IsFailure)
            return consumeOutcome.Fault;

        string refreshTokenKey = secureTokenGenerator.GenerateToken(SessionConstants.RefreshTokenKeyLength);
        string refreshToken = secureTokenGenerator.GenerateToken(SessionConstants.RefreshTokenLength);
        string refreshTokenHash = secureTokenGenerator.HashToken(refreshToken);

        Outcome<Session> sessionOutcome = Session.Create
        (
            userId: loginRequest.UserId,
            refreshTokenKey: refreshTokenKey,
            refreshTokenHash: refreshTokenHash,
            fingerprint: fingerprint,
            utcNow: dateTimeProvider.UtcNow
        );

        if (sessionOutcome.IsFailure)
            return sessionOutcome.Fault;

        Session session = sessionOutcome.Value;

        await dbContext.Sessions.AddAsync(session, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);

        string accessToken = tokenProvider.CreateToken(loginRequest.UserId.Value, session.Id.Value);

        VerifyLoginResponse verifyLoginResponse = new
        (
            AccessToken: accessToken,
            RefreshToken: $"{refreshTokenKey}.{refreshToken}"
        );

        return verifyLoginResponse;
    }
}
