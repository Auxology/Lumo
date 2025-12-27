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
        CancellationToken cancellationToken = default) where TData : class
    {
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

        logger.LogInformation("Sending templated email to {RecipientEmail} using template {TemplateName}",
            recipientEmailAddress, templateName);

        await sesService.SendEmailAsync(request, cancellationToken);

        logger.LogInformation("Templated email sent to {RecipientEmail} using template {TemplateName}",
            recipientEmailAddress, templateName);
    }
}
