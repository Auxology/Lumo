namespace Notifications.Api.Models;

internal sealed record EmailChangeRequestedTemplateData
{
    public required string CurrentEmailAddress { get; init; }

    public required string NewEmailAddress { get; init; }

    public required string OtpToken { get; init; }

    public required DateTimeOffset ExpiresAt { get; init; }

    public required string IpAddress { get; init; }

    public required string UserAgent { get; init; }

    public required string ApplicationName { get; init; }

    public override string ToString() =>
        $"EmailChangeRequestedTemplateData {{ CurrentEmailAddress=[REDACTED], NewEmailAddress=[REDACTED], OtpToken=[REDACTED], ExpiresAt={ExpiresAt}, IpAddress=[REDACTED], UserAgent={UserAgent}, ApplicationName={ApplicationName} }}";
}