using Main.Application.Abstractions.Data;
using Main.Application.Faults;
using Main.Domain.Aggregates;
using Main.Domain.ValueObjects;

using Microsoft.EntityFrameworkCore;

using SharedKernel;
using SharedKernel.Application.Authentication;
using SharedKernel.Application.Messaging;

namespace Main.Application.Chats.Update;

internal sealed class UpdateChatHandler(
    IMainDbContext dbContext,
    IUserContext userContext,
    IDateTimeProvider dateTimeProvider) : ICommandHandler<UpdateChatCommand, UpdateChatResponse>
{
    public async ValueTask<Outcome<UpdateChatResponse>> Handle(UpdateChatCommand request, CancellationToken cancellationToken)
    {
        Guid userId = userContext.UserId;
        Outcome<ChatId> chatIdOutcome = ChatId.From(request.ChatId);

        if (chatIdOutcome.IsFailure)
            return chatIdOutcome.Fault;

        ChatId chatId = chatIdOutcome.Value;

        Chat? chat = await dbContext.Chats
            .FirstOrDefaultAsync(c => c.Id == chatId && c.UserId == userId, cancellationToken);

        if (chat is null)
            return ChatOperationFaults.NotFound;

        if (request.IsArchived is true)
        {
            Outcome archiveOutcome = chat.Archive(dateTimeProvider.UtcNow);

            if (archiveOutcome.IsFailure)
                return archiveOutcome.Fault;
        }
        else if (request.IsArchived is false)
        {
            Outcome unarchiveOutcome = chat.Unarchive(dateTimeProvider.UtcNow);

            if (unarchiveOutcome.IsFailure)
                return unarchiveOutcome.Fault;
        }

        if (request.NewTitle is not null)
        {
            Outcome titleOutcome = chat.RenameTitle(request.NewTitle, dateTimeProvider.UtcNow);

            if (titleOutcome.IsFailure)
                return titleOutcome.Fault;
        }

        await dbContext.SaveChangesAsync(cancellationToken);

        UpdateChatResponse response = new
        (
            ChatId: chat.Id.Value,
            Title: chat.Title,
            IsArchived: chat.IsArchived,
            UpdatedAt: chat.UpdatedAt ?? dateTimeProvider.UtcNow
        );

        return response;
    }
}