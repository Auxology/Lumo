namespace Notifications.Api.Models;

internal sealed record UserDeletionRequestedTemplateData
{
    public required string WillBeDeletedAt { get; init; }

    public required string IpAddress { get; init; }

    public required string UserAgent { get; init; }

    public required string ApplicationName { get; init; }

    public override string ToString() =>
        $"UserDeletionRequestedTemplateData {{ WillBeDeletedAt={WillBeDeletedAt}, IpAddress=[REDACTED], UserAgent=[REDACTED], ApplicationName={ApplicationName} }}";
}