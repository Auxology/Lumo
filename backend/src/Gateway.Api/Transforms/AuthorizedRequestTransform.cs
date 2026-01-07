using System.Net.Http.Headers;

using Gateway.Api.Authentication;
using Gateway.Api.Extensions;

using SharedKernel;

using Yarp.ReverseProxy.Transforms;

namespace Gateway.Api.Transforms;

internal sealed class AuthorizedRequestTransform(ISessionTokenOrchestrator sessionTokenOrchestrator) : RequestTransform
{
    public override async ValueTask ApplyAsync(RequestTransformContext context)
    {
        HttpContext httpContext = context.HttpContext;

        string? refreshToken = httpContext.GetRefreshTokenCookie();

        if (refreshToken is null)
            return;

        Outcome<TokenPair> outcome = await sessionTokenOrchestrator.ResolveAccessTokenAsync(refreshToken);

        if (outcome.IsFailure)
            return;

        TokenPair tokenPair = outcome.Value;

        if (refreshToken != tokenPair.RefreshToken)
        {
            httpContext.RemoveRefreshTokenCookie();
            httpContext.SetRefreshTokenCookie(tokenPair.RefreshToken);
        }

        string accessToken = tokenPair.AccessToken;

        context.ProxyRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
    }
}