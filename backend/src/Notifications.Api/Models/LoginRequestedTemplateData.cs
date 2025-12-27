namespace Notifications.Api.Models;

internal sealed record LoginRequestedEmailTemplateData
{

    public required string OtpCode { get; init; }

    public required string MagicLink { get; init; }

    public required string ExpiresIn { get; init; }

    public required string IpAddress { get; init; }

    public required string Device { get; init; }

    public required string ApplicationName { get; init; }
}
