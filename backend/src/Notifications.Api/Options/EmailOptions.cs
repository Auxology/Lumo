using System.ComponentModel.DataAnnotations;

namespace Notifications.Api.Options;

internal sealed class EmailOptions
{
    public const string SectionName = "Email";

    [Required, EmailAddress]
    public string SenderEmail { get; init; } = string.Empty;

    [Required, MinLength(1)]
    public string ApplicationName { get; init; } = string.Empty;

    public string WelcomeEmailTemplateName { get; init; } = "WelcomeEmail";

    public string LoginRequestedTemplateName { get; init; } = "LoginRequested";

    public string LoginVerifiedTemplateName { get; init; } = "LoginVerified";

}
