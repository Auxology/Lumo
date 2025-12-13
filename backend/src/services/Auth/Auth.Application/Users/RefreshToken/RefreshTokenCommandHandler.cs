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

namespace Auth.Application.Users.RefreshToken;

internal sealed class RefreshTokenCommandHandler(
    IAuthDbContext dbContext,
    IRequestContext requestContext,
    ISecretHasher secretHasher,
    IJwtTokenProvider jwtTokenProvider,
    IDateTimeProvider dateTimeProvider ) : ICommandHandler<RefreshTokenCommand, RefreshTokenResponse>
{
    public async Task<Result<RefreshTokenResponse>> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
    {
        string[] parts = request.RefreshToken.Split(SessionConstants.RefreshTokenSeparator);

        if (parts is not { Length: 2 } || !Guid.TryParse(parts[0], out Guid tokenId))
            return SessionOperationErrors.InvalidRefreshToken;

        string secret = parts[1];

        Session? session = await dbContext.Sessions
            .FirstOrDefaultAsync(s => s.TokenId == tokenId, cancellationToken);

        if (session is null)
            return SessionOperationErrors.InvalidRefreshToken;

        if (!session.IsValid(dateTimeProvider))
            return SessionOperationErrors.InvalidRefreshToken;

        bool isSecretValid = secretHasher.Verify(secret, session.HashedSecret);

        if (!isSecretValid)
            return SessionOperationErrors.InvalidRefreshToken;

        User? user = await dbContext.Users
            .FirstOrDefaultAsync(u => u.Id == session.UserId, cancellationToken);

        if (user is null)
            return SessionOperationErrors.InvalidRefreshToken;

        EmailAddress emailAddress = user.EmailAddress;

        string newSecret = jwtTokenProvider.GenerateSecret();

        string newHashedSecret = secretHasher.Hash(newSecret);

        Result refreshResult = session.Refresh
        (
            newHashedSecret: newHashedSecret,
            ipAddress: requestContext.ClientIp,
            userAgent: requestContext.UserAgent,
            dateTimeProvider: dateTimeProvider
        );

        if (refreshResult.IsFailure)
            return refreshResult.Error;

        await dbContext.SaveChangesAsync(cancellationToken);

        TokenClaims tokenClaims = new
        (
            UserId: session.UserId.Value,
            SessionId: session.Id.Value,
            EmailAddress: emailAddress.Value
        );

        string accessToken = jwtTokenProvider.CreateAccessToken(tokenClaims);

        string newRefreshToken = $"{session.TokenId}{SessionConstants.RefreshTokenSeparator}{newSecret}";

        RefreshTokenResponse response = new
        (
            AccessToken: accessToken,
            RefreshToken: newRefreshToken
        );

        return response;
    }
}
