using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.HttpOverrides;
using SharedKernel.Api.Middleware;

namespace SharedKernel.Api.Extensions;

public static class MiddlewareExtensions
{
    public static IApplicationBuilder UseRequestEnrichment(this IApplicationBuilder app)
    {
        app.UseForwardedHeaders(new ForwardedHeadersOptions
        {
            ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
        });

        app.UseMiddleware<RequestEnrichmentMiddleware>();

        return app;
    }
}