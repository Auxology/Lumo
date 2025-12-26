using System.Net;
using Gateway.Api.Extensions;
using Yarp.ReverseProxy.Transforms;

namespace Gateway.Api.Transforms;

internal sealed class AuthorizedResponseTransform : ResponseTransform
{
    public override ValueTask ApplyAsync(ResponseTransformContext context)
    {
        HttpContext httpContext = context.HttpContext;

        if (context.ProxyResponse?.StatusCode == HttpStatusCode.Unauthorized)
            httpContext.RemoveRefreshTokenCookie();

        return ValueTask.CompletedTask;
    }
}
