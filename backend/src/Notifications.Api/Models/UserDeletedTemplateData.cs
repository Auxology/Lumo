namespace Notifications.Api.Models;

internal sealed record UserDeletedTemplateData
{
    public required string ApplicationName { get; init; }

    public override string ToString() =>
        $"UserDeletedTemplateData {{ ApplicationName={ApplicationName} }}";
}