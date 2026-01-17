namespace Notifications.Api.Models;

internal sealed record EmailAddressChangedTemplateData
{
    public required string OldEmailAddress { get; init; }

    public required string NewEmailAddress { get; init; }

    public required string ApplicationName { get; init; }

    public override string ToString() =>
        $"EmailAddressChangedTemplateData {{ OldEmailAddress=[REDACTED], NewEmailAddress=[REDACTED], ApplicationName={ApplicationName} }}";
}