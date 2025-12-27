namespace Notifications.Api.Models;

internal sealed record LoginVerifiedEmailTemplateData
{
    public required string LoginTime { get; init; }

    public required string IpAddress { get; init; }

    public required string Device { get; init; }

    public required string Location { get; init; }

    public required string ApplicationName { get; init; }

    public override string ToString() =>
        $"LoginVerifiedEmailTemplateData {{ LoginTime={LoginTime}, IpAddress=[REDACTED], Device={Device}, Location=[REDACTED], ApplicationName={ApplicationName} }}";
}
