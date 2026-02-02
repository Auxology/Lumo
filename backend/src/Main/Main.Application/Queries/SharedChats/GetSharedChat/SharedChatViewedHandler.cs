using Main.Application.Abstractions.Data;
using Main.Domain.ValueObjects;

using Mediator;

using Microsoft.EntityFrameworkCore;

using SharedKernel;

namespace Main.Application.Queries.SharedChats.GetSharedChat;

internal sealed class SharedChatViewedHandler(IMainDbContext dbContext, IDateTimeProvider dateTimeProvider)
    : INotificationHandler<SharedChatViewedNotification>
{
    public async ValueTask Handle(SharedChatViewedNotification notification, CancellationToken cancellationToken)
    {
        Outcome<SharedChatId> sharedChatIdOutcome = SharedChatId.From(notification.SharedChatId);

        if (sharedChatIdOutcome.IsFailure)
            return;

        SharedChatId sharedChatId = sharedChatIdOutcome.Value;

        await dbContext.SharedChats
            .Where(sc => sc.Id == sharedChatId)
            .ExecuteUpdateAsync(s =>
            {
                s.SetProperty(sc => sc.ViewCount, sc => sc.ViewCount + 1);
                s.SetProperty(sc => sc.UpdatedAt, dateTimeProvider.UtcNow);
            }, cancellationToken: cancellationToken);
    }
}