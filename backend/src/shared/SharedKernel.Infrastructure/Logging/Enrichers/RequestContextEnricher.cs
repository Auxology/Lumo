using Serilog.Core;
using Serilog.Events;
using SharedKernel.Application.Context;

namespace SharedKernel.Infrastructure.Logging.Enrichers;

public sealed class RequestContextEnricher(IRequestContext requestContext) : ILogEventEnricher
{
    public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
    {
        ArgumentNullException.ThrowIfNull(logEvent);
        ArgumentNullException.ThrowIfNull(propertyFactory);
        
        AddPropertyIfNotNull(logEvent, propertyFactory, "Browser", requestContext.Browser);
        
        AddPropertyIfNotNull(logEvent, propertyFactory, "OperatingSystem", requestContext.OperatingSystem);
        
        AddPropertyIfNotNull(logEvent, propertyFactory, "Device", requestContext.Device);
        
        logEvent.AddPropertyIfAbsent(propertyFactory.CreateProperty("IsMobile", requestContext.IsMobile));    
    }

    private static void AddPropertyIfNotNull
    (
        LogEvent logEvent,
        ILogEventPropertyFactory propertyFactory,
        string propertyName,
        string? propertyValue
    )
    {
        if (!string.IsNullOrWhiteSpace(propertyValue))
        {
            LogEventProperty property = propertyFactory.CreateProperty(propertyName, propertyValue);
            logEvent.AddPropertyIfAbsent(property);
        }
    }
}
