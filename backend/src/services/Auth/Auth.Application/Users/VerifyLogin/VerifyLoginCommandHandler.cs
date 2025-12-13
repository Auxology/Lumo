using Auth.Application.Abstractions.Authentication;
using Auth.Application.Abstractions.Data;
using Auth.Application.Errors;
using Auth.Domain.Aggregates.SessionAggregate;
using Auth.Domain.Aggregates.UserAggregate;
using Auth.Domain.Constants;
using Auth.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Shared.Contracts.Authentication;
using SharedKernel.Application.Context;
using SharedKernel.Application.Messaging;
using SharedKernel.Authentication;
using SharedKernel.ResultPattern;
using SharedKernel.Time;

namespace Auth.Application.Users.VerifyLogin;

internal sealed class VerifyLoginCommandHandler(
    IAuthDbContext dbContext,
    IRequestContext requestContext,
    ISecretHasher secretHasher,
    IJwtTokenProvider jwtTokenProvider,
    IDateTimeProvider dateTimeProvider) : ICommandHandler<VerifyLoginCommand, VerifyLoginResponse>
{
    public async Task<Result<VerifyLoginResponse>> Handle(VerifyLoginCommand request, CancellationToken cancellationToken)
    {
        Result<EmailAddress> emailResult = EmailAddress.Create(request.EmailAddress);

        if (emailResult.IsFailure)
            return emailResult.Error;

        EmailAddress emailAddress = emailResult.Value;

        User? user = await dbContext.Users
            .FirstOrDefaultAsync(u => u.EmailAddress == emailAddress, cancellationToken);

        if (user is null)
            return UserOperationErrors.InvalidCredentials;

        Result verifyResult = user.VerifyLogin
        (
            otpToken: request.OtpToken,
            magicLinkToken: request.MagicLinkToken,
            secretHasher: secretHasher,
            dateTimeProvider: dateTimeProvider
        );

        if (verifyResult.IsFailure)
            return verifyResult.Error;

        string secret = jwtTokenProvider.GenerateSecret();

        string hashedSecret = secretHasher.Hash(secret);

        Result<Session> sessionResult = Session.Create
        (
            userId: user.Id,
            hashedSecret: hashedSecret,
            ipAddress: requestContext.ClientIp,
            userAgent: requestContext.UserAgent,
            dateTimeProvider: dateTimeProvider
        );

        if (sessionResult.IsFailure)
            return sessionResult.Error;

        Session session = sessionResult.Value;

        await dbContext.Sessions.AddAsync(session, cancellationToken);

        await dbContext.SaveChangesAsync(cancellationToken);

        TokenClaims tokenClaims = new
        (
            UserId: user.Id.Value,
            SessionId: session.Id.Value,
            EmailAddress: user.EmailAddress.Value
        );

        string accessToken = jwtTokenProvider.CreateAccessToken(tokenClaims);

        string refreshToken = $"{session.TokenId}{SessionConstants.RefreshTokenSeparator}{secret}";

        VerifyLoginResponse response = new
        (
            AccessToken: accessToken,
            RefreshToken: refreshToken
        );

        return response;
    }
}
