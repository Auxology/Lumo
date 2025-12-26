using Contracts.Requests;
using Contracts.Responses;
using Gateway.Api.Caching;
using Gateway.Api.HttpClients;
using SharedKernel;
using SharedKernel.Application.Authentication;

namespace Gateway.Api.Authentication;

internal sealed class SessionTokenOrchestrator(
    IAuthServiceClient authServiceClient,
    ITokenCacheService tokenCacheService,
    IJwtTokenValidator jwtTokenValidator) : ISessionTokenOrchestrator
{
    public async Task<Outcome<string>> VerifyLoginAsync(VerifyLoginApiRequest request, CancellationToken cancellationToken = default)
    {
        Outcome<VerifyLoginApiResponse> outcome =
            await authServiceClient.VerifyLoginAsync(request, cancellationToken);

        if (outcome.IsFailure)
            return outcome.Fault;

        await tokenCacheService.SetAccessTokenAsync
        (
            refreshToken: outcome.Value.RefreshToken,
            accessToken: outcome.Value.AccessToken,
            cancellationToken: cancellationToken
        );

        return outcome.Value.RefreshToken;
    }

    public async Task<Outcome<TokenPair>> ResolveAccessTokenAsync(string refreshToken, CancellationToken cancellationToken = default)
    {
        string? cachedToken = await tokenCacheService.GetAccessTokenAsync(refreshToken, cancellationToken);

        if (cachedToken is not null && await jwtTokenValidator.IsValidAsync(cachedToken, cancellationToken))
        {
            return new TokenPair(cachedToken, refreshToken);
        }

        RefreshSessionApiRequest request = new(refreshToken);

        Outcome<RefreshSessionApiResponse> outcome =
            await authServiceClient.RefreshSessionAsync(request, cancellationToken);

        if (outcome.IsFailure)
            return outcome.Fault;

        RefreshSessionApiResponse response = outcome.Value;

        await tokenCacheService.SetAccessTokenAsync
        (
            refreshToken: response.RefreshToken,
            accessToken: response.AccessToken,
            cancellationToken: cancellationToken
        );

        return new TokenPair(response.AccessToken, response.RefreshToken);
    }
}
