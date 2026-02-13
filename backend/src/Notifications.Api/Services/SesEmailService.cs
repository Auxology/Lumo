using System.Text.Json;

using Amazon.SimpleEmailV2;
using Amazon.SimpleEmailV2.Model;

using Microsoft.Extensions.Options;

using Notifications.Api.Options;

namespace Notifications.Api.Services;

internal sealed class SesEmailService(
    IAmazonSimpleEmailServiceV2 sesService,
    IOptions<EmailOptions> emailOptions,
    ILogger<SesEmailService> logger) : IEmailService
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    private readonly EmailOptions _emailOptions = emailOptions.Value;

    public async Task SendTemplatedEmailAsync<TData>(string recipientEmailAddress, string templateName, TData templateData,
        CancellationToken cancellationToken = default) where TData : notnull
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(recipientEmailAddress);
        ArgumentException.ThrowIfNullOrWhiteSpace(templateName);
        ArgumentNullException.ThrowIfNull(templateData);

        string templateDataJson = JsonSerializer.Serialize(templateData, JsonOptions);

        SendEmailRequest request = new()
        {
            FromEmailAddress = _emailOptions.SenderEmail,
            Destination = new Destination()
            {
                ToAddresses = [recipientEmailAddress]
            },
            Content = new EmailContent()
            {
                Template = new Template()
                {
                    TemplateName = templateName,
                    TemplateData = templateDataJson
                }
            }
        };

        if (logger.IsEnabled(LogLevel.Information))
            logger.LogInformation("Sending templated email to recipient. Template: {TemplateName}", templateName);

        SendEmailResponse response = await sesService.SendEmailAsync(request, cancellationToken);

        if (logger.IsEnabled(LogLevel.Information))
            logger.LogInformation(
                "Templated email sent using template {TemplateName}. MessageId: {MessageId}, HttpStatusCode: {StatusCode}",
                templateName, response.MessageId, response.HttpStatusCode);
    }
}