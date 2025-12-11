using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Serilog.Core;
using Serilog.Events;
using SharedKernel.Application.Authentication;

namespace SharedKernel.Infrastructure.Logging.Enrichers;

public sealed class UserContextEnricher(IHttpContextAccessor httpContextAccessor) : ILogEventEnricher
{
    public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
    {
        ArgumentNullException.ThrowIfNull(logEvent);
        ArgumentNullException.ThrowIfNull(propertyFactory);

        HttpContext? httpContext = httpContextAccessor.HttpContext;
        
        if (httpContext is null)
            return;
        
        IUserContext? userContext = httpContext.RequestServices.GetService<IUserContext>();
        
        if (userContext is null)
            return;
        
        try
        {
            Guid userId = userContext.UserId;
            Guid sessionId = userContext.SessionId;
            
            AddPropertyIfNotNull(logEvent, propertyFactory, "UserId", userId.ToString("N"));
            AddPropertyIfNotNull(logEvent, propertyFactory, "SessionId", sessionId.ToString("N"));
        }
        catch (InvalidOperationException)
        {
            // User not authenticated - skip enrichment
        }
    }
    
    private static void AddPropertyIfNotNull
    (
        LogEvent logEvent,
        ILogEventPropertyFactory propertyFactory,
        string propertyName,
        string propertyValue
    )
    {
        if (!string.IsNullOrWhiteSpace(propertyValue))
        {
            LogEventProperty property = propertyFactory.CreateProperty(propertyName, propertyValue);
            logEvent.AddPropertyIfAbsent(property);
        }
    }
}