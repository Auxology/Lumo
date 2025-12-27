namespace Notifications.Api.Models;

internal sealed record WelcomeEmailTemplateData
{
    public required string DisplayName { get; init; }

    public required string ApplicationName { get; init; }
}
