using Contracts.IntegrationEvents.Auth;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Notifications.Api.Data;
using Notifications.Api.Entities;
using Notifications.Api.Models;
using Notifications.Api.Options;
using Notifications.Api.Services;
using SharedKernel;

namespace Notifications.Api.Consumers;

internal sealed class LoginRequestedConsumer(
    INotificationDbContext notificationDbContext,
    IEmailService emailService,
    IOptions<EmailOptions> emailOptions,
    IDateTimeProvider dateTimeProvider) : IConsumer<LoginRequested>
{
    private readonly EmailOptions _emailOptions = emailOptions.Value;

    public async Task Consume(ConsumeContext<LoginRequested> context)
    {
        CancellationToken cancellationToken = context.CancellationToken;
        Guid eventId = context.Message.EventId;

        bool alreadyProcessed = await notificationDbContext.ProcessedEvents
            .AnyAsync(e => e.EventId == eventId, cancellationToken);

        if (alreadyProcessed)
            return;

        ProcessedEvent processedEvent = ProcessedEvent.Create(eventId, dateTimeProvider.UtcNow);

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

        try
        {
            await notificationDbContext.ProcessedEvents.AddAsync(processedEvent, cancellationToken);
            await notificationDbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException)
        {
            return;
        }
    }
}
