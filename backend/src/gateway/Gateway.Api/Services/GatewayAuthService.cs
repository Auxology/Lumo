using Gateway.Api.Cache;
using Gateway.Api.Clients;
using Shared.Contracts.Authentication;
using SharedKernel.ResultPattern;

namespace Gateway.Api.Services;

internal sealed class GatewayAuthService(IAuthServiceClient authServiceClient, IAccessTokenCache accessTokenCache) : IGatewayAuthService
{
    public async Task<Result<string>> VerifyLoginAsync(CreateSessionRequest request, CancellationToken cancellationToken = default)
    {
        Result<VerifyLoginResponse> result = await authServiceClient.CallVerifyLoginAsync(request, cancellationToken);

        if (result.IsFailure)
            return result.Error;

        string accessToken = result.Value.AccessToken;

        string refreshToken = result.Value.RefreshToken;

        await accessTokenCache.StoreAccessTokenAsync(refreshToken, accessToken, cancellationToken);

        return refreshToken;
    }
}
