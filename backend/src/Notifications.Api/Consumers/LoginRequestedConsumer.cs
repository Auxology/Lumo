using Contracts.IntegrationEvents.Auth;

using MassTransit;

using Microsoft.Extensions.Options;

using Notifications.Api.Models;
using Notifications.Api.Options;
using Notifications.Api.Services;

namespace Notifications.Api.Consumers;

internal sealed class LoginRequestedConsumer(
    IEmailService emailService,
    IOptions<EmailOptions> emailOptions) : IConsumer<LoginRequested>
{
    private readonly EmailOptions _emailOptions = emailOptions.Value;

    public async Task Consume(ConsumeContext<LoginRequested> context)
    {
        CancellationToken cancellationToken = context.CancellationToken;

        LoginRequestedEmailTemplateData templateData = new()
        {
            OtpToken = context.Message.OtpToken,
            MagicLinkToken = context.Message.MagicLinkToken,
            ExpiresAt = context.Message.ExpiresAt,
            IpAddress = context.Message.IpAddress,
            UserAgent = context.Message.UserAgent,
            ApplicationName = _emailOptions.ApplicationName,
            FrontendUrl = _emailOptions.FrontendUrl
        };

        await emailService.SendTemplatedEmailAsync
        (
            recipientEmailAddress: context.Message.EmailAddress,
            templateName: _emailOptions.LoginRequestedTemplateName,
            templateData: templateData,
            cancellationToken: cancellationToken
        );
    }
}