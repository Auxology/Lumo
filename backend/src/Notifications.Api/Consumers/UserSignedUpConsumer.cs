using Contracts.IntegrationEvents.Auth;

using MassTransit;

using Microsoft.Extensions.Options;

using Notifications.Api.Models;
using Notifications.Api.Options;
using Notifications.Api.Services;

namespace Notifications.Api.Consumers;

internal sealed class UserSignedUpConsumer(
    IEmailService emailService,
    IOptions<EmailOptions> emailOptions) : IConsumer<UserSignedUp>
{
    private readonly EmailOptions _emailOptions = emailOptions.Value;

    public async Task Consume(ConsumeContext<UserSignedUp> context)
    {
        CancellationToken cancellationToken = context.CancellationToken;

        WelcomeEmailTemplateData templateData = new()
        {
            DisplayName = context.Message.DisplayName,
            ApplicationName = _emailOptions.ApplicationName
        };

        await emailService.SendTemplatedEmailAsync
        (
            recipientEmailAddress: context.Message.EmailAddress,
            templateName: _emailOptions.WelcomeEmailTemplateName,
            templateData: templateData,
            cancellationToken: cancellationToken
        );
    }
}