using Shared.Contracts.Authentication;
using SharedKernel.ResultPattern;

namespace Gateway.Api.Services;

internal interface IGatewayAuthService
{
    Task<Result<string>> VerifyLoginAsync(CreateSessionRequest request, CancellationToken cancellationToken = default);

    Task<Result<TokenPair>> RefreshTokensAsync(string refreshToken, CancellationToken cancellationToken = default);
}

internal sealed record TokenPair
(
    string AccessToken,
    string RefreshToken
);
