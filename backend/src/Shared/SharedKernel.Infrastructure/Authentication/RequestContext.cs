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

    private ClientInfo? _clientInfo;

    public RequestContext(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    private HttpContext HttpContext => _httpContextAccessor.HttpContext
                                   ?? throw new InvalidOperationException("HTTP context is unavailable");

    public string IpAddress => GetClientIpAddress();

    public string UserAgent => HttpContext.Request.Headers.UserAgent.ToString();

    public string Timezone => GetTimezone();

    public string Language => GetPreferredLanguage();

    public string NormalizedBrowser => (_clientInfo ??= UaParser.Parse(UserAgent)).UA.Family;

    public string NormalizedOs => (_clientInfo ??= UaParser.Parse(UserAgent)).OS.Family;

    public string CorrelationId => field ??= GetOrCreateCorrelationId();

    private string GetClientIpAddress()
    {
        string forwardedFor = HttpContext.Request.Headers[ForwardedForHeader].ToString();

        if (!string.IsNullOrEmpty(forwardedFor))
            return forwardedFor.Split(',')[0].Trim();

        return HttpContext.Connection.RemoteIpAddress?.ToString() ?? string.Empty;
    }

    private string GetTimezone()
    {
        string timezone = HttpContext.Request.Headers[TimezoneHeader].ToString();

        if (string.IsNullOrWhiteSpace(timezone))
            return "UTC";

        return timezone.Trim();
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
            !string.IsNullOrEmpty(correlationId) &&
            Guid.TryParse(correlationId, out _))
        {
            return correlationId.ToString();
        }

        // TraceIdentifier is not used because it's not a valid GUID format (e.g., "0HMPNHL0JH8FL:00000001")
        // and CorrelationId is expected to be a parseable GUID elsewhere in the system.
        return Guid.NewGuid().ToString();
    }
}
