namespace Notifications.Api.Services;

internal interface IEmailService
{
    Task SendTemplatedEmailAsync<TData>
    (
        string recipientEmailAddress,
        string templateName,
        TData templateData,
        CancellationToken cancellationToken = default
    ) where TData : class;
}
