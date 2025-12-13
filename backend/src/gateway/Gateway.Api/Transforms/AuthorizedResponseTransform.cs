using Gateway.Api.Extensions;
using SharedKernel.Time;
using Yarp.ReverseProxy.Transforms;

namespace Gateway.Api.Transforms;

internal sealed class AuthorizedResponseTransform(IDateTimeProvider dateTimeProvider) : ResponseTransform
{
#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
    public override async ValueTask ApplyAsync(ResponseTransformContext context)
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
    {
        HttpContext httpContext = context.HttpContext;

        if (context.ProxyResponse?.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            httpContext.RemoveRefreshTokenCookie(dateTimeProvider);
    }
}
