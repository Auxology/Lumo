using Contracts.IntegrationEvents.Auth;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Options;
using Notifications.Api.Data;
using Notifications.Api.Entities;
using Notifications.Api.Models;
using Notifications.Api.Options;
using Notifications.Api.Services;
using SharedKernel;

namespace Notifications.Api.Consumers;

internal sealed class UserSignedUpConsumer(
    INotificationDbContext notificationDbContext,
    IEmailService emailService,
    IOptions<EmailOptions> emailOptions,
    IDateTimeProvider dateTimeProvider) : IConsumer<UserSignedUp>
{
    private readonly EmailOptions _emailOptions = emailOptions.Value;

    public async Task Consume(ConsumeContext<UserSignedUp> context)
    {
        CancellationToken cancellationToken = context.CancellationToken;
        Guid eventId = context.Message.EventId;

        bool alreadyProcessed = await notificationDbContext.ProcessedEvents
            .AnyAsync(e => e.EventId == eventId, cancellationToken);

        if (alreadyProcessed)
            return;

        ProcessedEvent processedEvent = ProcessedEvent.Create(eventId, dateTimeProvider.UtcNow);

        await using IDbContextTransaction transaction =
            await notificationDbContext.Database.BeginTransactionAsync(cancellationToken);

        try
        {
            await notificationDbContext.ProcessedEvents.AddAsync(processedEvent, cancellationToken);
            await notificationDbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException)
        {
            return;
        }

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

        await transaction.CommitAsync(cancellationToken);
    }
}
