namespace Notifications.Api.Models;

internal sealed record UserDeletionCanceledTemplateData
{
    public required string ApplicationName { get; init; }

    public override string ToString() =>
        $"UserDeletionCanceledTemplateData {{ ApplicationName={ApplicationName} }}";
}
