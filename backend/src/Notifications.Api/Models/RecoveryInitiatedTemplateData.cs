namespace Notifications.Api.Models;

internal sealed record RecoveryInitiatedTemplateData
{
    public required string OldEmailAddress { get; init; }

    public required string NewEmailAddress { get; init; }

    public required string OtpToken { get; init; }

    public required string MagicLinkToken { get; init; }

    public required DateTimeOffset ExpiresAt { get; init; }

    public required string IpAddress { get; init; }

    public required string UserAgent { get; init; }

    public required string ApplicationName { get; init; }

    public required string FrontendUrl { get; init; }

    public string MagicLinkUrl => $"{FrontendUrl.TrimEnd('/')}/auth/recovery/verify?token={Uri.EscapeDataString(MagicLinkToken)}";

    public override string ToString() =>
        $"RecoveryInitiatedTemplateData {{ OldEmailAddress=[REDACTED], NewEmailAddress=[REDACTED], OtpToken=[REDACTED], MagicLinkToken=[REDACTED], ExpiresAt={ExpiresAt}, IpAddress=[REDACTED], UserAgent={UserAgent}, ApplicationName={ApplicationName}, FrontendUrl={FrontendUrl} }}";
}