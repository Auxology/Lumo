namespace Notifications.Api.Models;

internal sealed record LoginRequestedEmailTemplateData
{
    public required string OtpToken { get; init; }

    public required string MagicLinkToken { get; init; }

    public required DateTimeOffset ExpiresAt { get; init; }

    public required string IpAddress { get; init; }

    public required string UserAgent { get; init; }

    public required string ApplicationName { get; init; }

    public required string FrontendUrl { get; init; }

    public string MagicLinkUrl => $"{FrontendUrl.TrimEnd('/')}/auth/magic-link?token={Uri.EscapeDataString(MagicLinkToken)}";

    public override string ToString() =>
        $"LoginRequestedEmailTemplateData {{ OtpToken=[REDACTED], MagicLinkToken=[REDACTED], ExpiresAt={ExpiresAt}, IpAddress=[REDACTED], UserAgent={UserAgent}, ApplicationName={ApplicationName}, FrontendUrl={FrontendUrl} }}";
}
