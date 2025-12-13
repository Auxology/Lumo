using Gateway.Api.Cache;
using Gateway.Api.Clients;
using Shared.Contracts.Authentication;
using SharedKernel.Application.Authentication;
using SharedKernel.ResultPattern;

namespace Gateway.Api.Services;

internal sealed class GatewayAuthService(
    IAuthServiceClient authServiceClient,
    IAccessTokenCache accessTokenCache,
    IJwtTokenValidator jwtTokenValidator) : IGatewayAuthService
{
    public async Task<Result<string>> VerifyLoginAsync(CreateSessionRequest request, CancellationToken cancellationToken = default)
    {
        Result<VerifyLoginResponse> result = await authServiceClient.CallVerifyLoginAsync(request, cancellationToken);

        if (result.IsFailure)
            return result.Error;

        string accessToken = result.Value.AccessToken;

        string refreshToken = result.Value.RefreshToken;

        await accessTokenCache.StoreAccessTokenAsync
        (
            refreshTokenKey: refreshToken,
            accessToken: accessToken,
            cancellationToken: cancellationToken
        );

        return refreshToken;
    }

    public async Task<Result<TokenPair>> RefreshTokensAsync(string refreshToken, CancellationToken cancellationToken = default)
    {
        string? accessToken = await accessTokenCache.GetAccessTokenAsync(refreshToken, cancellationToken);

        if (accessToken is null || !await jwtTokenValidator.IsValidAsync(accessToken, cancellationToken))
        {
            RefreshSessionRequest refreshSessionRequest = new(refreshToken);

            Result<RefreshTokenResponse> result = await authServiceClient.CallRefreshTokenAsync(refreshSessionRequest, cancellationToken);

            if (result.IsFailure)
                return result.Error;

            string newAccessToken = result.Value.AccessToken;

            string newRefreshToken = result.Value.RefreshToken;

            await accessTokenCache.StoreAccessTokenAsync
            (
                refreshTokenKey: newRefreshToken,
                accessToken: newAccessToken,
                cancellationToken: cancellationToken
            );

            TokenPair newTokenPair = new
            (
                AccessToken: newAccessToken,
                RefreshToken: newRefreshToken
            );

            return newTokenPair;
        }

        TokenPair existingTokenPair = new
        (
            AccessToken: accessToken,
            RefreshToken: refreshToken
        );

        return existingTokenPair;
    }
}
