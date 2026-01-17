using Contracts.IntegrationEvents.Auth;

using MassTransit;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using Notifications.Api.Models;
using Notifications.Api.Options;
using Notifications.Api.Services;

namespace Notifications.Api.Consumers;

internal sealed class UserEmailAddressChangedConsumer(
    IEmailService emailService,
    IOptions<EmailOptions> emailOptions,
    ILogger<UserEmailAddressChangedConsumer> logger) : IConsumer<UserEmailAddressChanged>
{
    private readonly EmailOptions _emailOptions = emailOptions.Value;

    public async Task Consume(ConsumeContext<UserEmailAddressChanged> context)
    {
        CancellationToken cancellationToken = context.CancellationToken;
        UserEmailAddressChanged message = context.Message;

        EmailAddressChangedTemplateData templateData = new()
        {
            OldEmailAddress = message.OldEmailAddress,
            NewEmailAddress = message.NewEmailAddress,
            ApplicationName = _emailOptions.ApplicationName
        };

        await emailService.SendTemplatedEmailAsync
        (
            recipientEmailAddress: message.OldEmailAddress,
            templateName: _emailOptions.EmailAddressChangedTemplateName,
            templateData: templateData,
            cancellationToken: cancellationToken
        );

        logger.LogInformation(
            "Consumed {EventType}: {EventId}, CorrelationId: {CorrelationId}, OccurredAt: {OccurredAt}, UserId: {UserId}",
            nameof(UserEmailAddressChanged), message.EventId, message.CorrelationId, message.OccurredAt, message.UserId);
    }
}