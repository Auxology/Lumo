using Microsoft.AspNetCore.Http;
using SharedKernel.Application.Authentication;
using UAParser;

namespace SharedKernel.Infrastructure.Authentication;

public sealed class RequestContext : IRequestContext
{
    private const string TimezoneHeader = "X-Timezone";
    private const string ForwardedForHeader = "X-Forwarded-For";
    private const string CorrelationIdHeader = "X-Correlation-ID";

    private static readonly Parser UaParser = Parser.GetDefault();

    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly Lazy<ClientInfo> _clientInfo;

    public RequestContext(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
        _clientInfo = new Lazy<ClientInfo>(() => UaParser.Parse(UserAgent));
    }

    private HttpContext HttpContext => _httpContextAccessor.HttpContext
                                   ?? throw new InvalidOperationException("HTTP context is unavailable");

    public string IpAddress => GetClientIpAddress();

    public string UserAgent => HttpContext.Request.Headers.UserAgent.ToString();

    public string Timezone => HttpContext.Request.Headers[TimezoneHeader].ToString();

    public string Language => GetPreferredLanguage();

    public string NormalizedBrowser => _clientInfo.Value.UA.Family;

    public string NormalizedOs => _clientInfo.Value.OS.Family;

    public string CorrelationId => GetOrCreateCorrelationId();

    private string GetClientIpAddress()
    {
        string forwardedFor = HttpContext.Request.Headers[ForwardedForHeader].ToString();

        if (!string.IsNullOrEmpty(forwardedFor))
            return forwardedFor.Split(',')[0].Trim();

        return HttpContext.Connection.RemoteIpAddress?.ToString() ?? string.Empty;
    }

    private string GetPreferredLanguage()
    {
        string acceptLanguage = HttpContext.Request.Headers.AcceptLanguage.ToString();

        if (string.IsNullOrEmpty(acceptLanguage))
            return string.Empty;

        return acceptLanguage.Split(',')[0].Split(';')[0].Trim();
    }

    private string GetOrCreateCorrelationId()
    {
        if (HttpContext.Request.Headers.TryGetValue(CorrelationIdHeader, out var correlationId) &&
            !string.IsNullOrEmpty(correlationId))
        {
            return correlationId.ToString();
        }

        return HttpContext.TraceIdentifier;
    }
}
