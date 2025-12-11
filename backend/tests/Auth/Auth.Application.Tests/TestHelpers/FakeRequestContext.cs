using SharedKernel.Application.Context;

namespace Auth.Application.Tests.TestHelpers;

public class FakeRequestContext : IRequestContext
{
    public Guid UserId { get; set; } = Guid.NewGuid();

    public string CorrelationId { get; set; } = Guid.NewGuid().ToString("N");

    public string? ClientIp { get; set; } = "127.0.0.1";

    public string? UserAgent { get; set; } = "Test User Agent";

    public string? Browser { get; set; } = "Chrome 120";

    public string? OperatingSystem { get; set; } = "Windows 10";

    public string? Device { get; set; } = "Other";

    public bool IsMobile { get; set; }

    public string? RequestPath { get; set; } = "/api/test";

    public string? RequestMethod { get; set; } = "GET";
}
