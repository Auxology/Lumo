using Microsoft.AspNetCore.Http;
using SharedKernel.Application.Context;
using SharedKernel.Constants;
using UAParser;

namespace SharedKernel.Infrastructure.Context;

public sealed class RequestContext(IHttpContextAccessor httpContextAccessor) : IRequestContext
{
    private HttpContext? HttpContext => httpContextAccessor.HttpContext;
    
    private static readonly Parser UaParser = Parser.GetDefault();
    
    private ClientInfo? _parsedUserAgent;
    
    private bool _userAgentParsed;

    public string CorrelationId
    {
        get
        {
            if (HttpContext is null)
                return Guid.CreateVersion7().ToString("N");
            
            if (HttpContext.Items.TryGetValue(CorrelationConstants.CorrelationIdKey, out var existing) && existing is string id)
                return id;

            if (HttpContext.Request.Headers.TryGetValue(CorrelationConstants.CorrelationIdHeader, out var headerValue) 
                && !string.IsNullOrWhiteSpace(headerValue))
                return headerValue.ToString();

            return HttpContext.TraceIdentifier;
        }
    }

    public string? ClientIp => HttpContext?.Connection.RemoteIpAddress?.ToString();
    
    public string? UserAgent => HttpContext?.Request.Headers.UserAgent.ToString();

    public string? Browser
    {
        get
        {
            EnsureUserAgentParsed();
            
            return _parsedUserAgent is null 
                ? null
                : $"{_parsedUserAgent.UA.Family} {_parsedUserAgent.UA.Major}".Trim();
        }
    }

    public string? OperatingSystem
    {
        get
        {
            EnsureUserAgentParsed();
            
            return _parsedUserAgent is null 
                ? null 
                : $"{_parsedUserAgent.OS.Family} {_parsedUserAgent.OS.Major}".Trim();
        }
    }

    public string? Device
    {
        get
        {
            EnsureUserAgentParsed();
            
            return _parsedUserAgent?.Device.Family;
        }
    }

    public bool IsMobile
    {
        get
        {
            EnsureUserAgentParsed();

            if (_parsedUserAgent is null)
                return false;
            
            string device = _parsedUserAgent.Device.Family;
            
            return device != "Other" && device != "Spider";
        }
    }

    public string? RequestPath => HttpContext?.Request.Path.Value;
    
    public string? RequestMethod => HttpContext?.Request.Method;

    private void EnsureUserAgentParsed()
    {
        if (_userAgentParsed)
            return;

        _userAgentParsed = true;
        
        string? ua = UserAgent;
        
        if (!string.IsNullOrWhiteSpace(ua))
            _parsedUserAgent = UaParser.Parse(ua);
    }
}