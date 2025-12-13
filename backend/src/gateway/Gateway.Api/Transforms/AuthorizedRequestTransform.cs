using System.Net.Http.Headers;
using Gateway.Api.Extensions;
using Gateway.Api.Services;
using SharedKernel.ResultPattern;
using SharedKernel.Time;
using Yarp.ReverseProxy.Transforms;

namespace Gateway.Api.Transforms;

internal sealed class AuthorizedRequestTransform(IGatewayAuthService gatewayAuthService, IDateTimeProvider dateTimeProvider) : RequestTransform
{
    public override async ValueTask ApplyAsync(RequestTransformContext context)
    {
        HttpContext httpContext = context.HttpContext;

        string? refreshToken = httpContext.GetRefreshTokenFromCookie();

        if (refreshToken is null)
            return;

        Result<TokenPair> result = await gatewayAuthService.GetOrRefreshTokenAsync(refreshToken);

        if (result.IsFailure)
            return;

        TokenPair tokenPair = result.Value;

        if (refreshToken != tokenPair.RefreshToken)
        {
            httpContext.RemoveRefreshTokenCookie(dateTimeProvider);

            httpContext.SetRefreshTokenCookie(tokenPair.RefreshToken, dateTimeProvider);
        }

        context.ProxyRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", tokenPair.AccessToken);
    }
}
