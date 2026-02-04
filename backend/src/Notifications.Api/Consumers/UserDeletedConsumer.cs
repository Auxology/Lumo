using Contracts.IntegrationEvents.Auth;

using MassTransit;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using Notifications.Api.Models;
using Notifications.Api.Options;
using Notifications.Api.Services;

namespace Notifications.Api.Consumers;

internal sealed class UserDeletedConsumer(
    IEmailService emailService,
    IOptions<EmailOptions> emailOptions,
    ILogger<UserDeletedConsumer> logger) : IConsumer<UserDeleted>
{
    private readonly EmailOptions _emailOptions = emailOptions.Value;

    public async Task Consume(ConsumeContext<UserDeleted> context)
    {
        CancellationToken cancellationToken = context.CancellationToken;
        UserDeleted message = context.Message;

        UserDeletedTemplateData templateData = new()
        {
            ApplicationName = _emailOptions.ApplicationName
        };

        await emailService.SendTemplatedEmailAsync
        (
            recipientEmailAddress: message.EmailAddress,
            templateName: _emailOptions.UserDeletedTemplateName,
            templateData: templateData,
            cancellationToken: cancellationToken
        );

        logger.LogInformation(
            "Consumed {EventType}: {EventId}, CorrelationId: {CorrelationId}, OccurredAt: {OccurredAt}, UserId: {UserId}",
            nameof(UserDeleted), message.EventId, message.CorrelationId, message.OccurredAt, message.UserId);
    }
}