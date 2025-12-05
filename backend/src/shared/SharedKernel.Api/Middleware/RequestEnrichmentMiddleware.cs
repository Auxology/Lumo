using Microsoft.AspNetCore.Http;
using Serilog.Context;
using SharedKernel.Constants;

namespace SharedKernel.Api.Middleware;

public sealed class RequestEnrichmentMiddleware(RequestDelegate next)
{
    public async Task InvokeAsync(HttpContext context)
    {
        ArgumentNullException.ThrowIfNull(context);
        
        string correlationId = GetOrCreateCorrelationId(context);
        
        context.Items[CorrelationConstants.CorrelationIdKey] = correlationId;
        
        context.Response.OnStarting(() =>
        {
            context.Response.Headers.TryAdd(CorrelationConstants.CorrelationIdHeader, correlationId);
            return Task.CompletedTask;
        });

        using (LogContext.PushProperty("CorrelationId", correlationId))
        using (LogContext.PushProperty("RequestPath", context.Request.Path.Value))
        using (LogContext.PushProperty("RequestMethod", context.Request.Method))
        {
            await next(context);
        }
    }
    
    private static string GetOrCreateCorrelationId(HttpContext context)
    {
        if (context.Request.Headers.TryGetValue(CorrelationConstants.CorrelationIdHeader, out var headerValue) 
            && !string.IsNullOrWhiteSpace(headerValue))
        {
            return headerValue.ToString();
        }

        return Guid.CreateVersion7().ToString("N");
    }
}