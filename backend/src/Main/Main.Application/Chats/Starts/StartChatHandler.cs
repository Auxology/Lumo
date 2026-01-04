using Main.Application.Abstractions.AI;
using Main.Application.Abstractions.Data;
using Main.Application.Faults;
using Main.Domain.Aggregates;
using Main.Domain.ReadModels;
using Microsoft.EntityFrameworkCore;
using SharedKernel;
using SharedKernel.Application.Authentication;
using SharedKernel.Application.Messaging;

namespace Main.Application.Chats.Starts;

internal sealed class StartChatHandler(
    IMainDbContext dbContext,
    IUserContext userContext,
    IChatCompletionService chatCompletionService,
    IDateTimeProvider dateTimeProvider) : ICommandHandler<StartChatCommand, StartChatResponse>
{
    public async ValueTask<Outcome<StartChatResponse>> Handle(StartChatCommand request, CancellationToken cancellationToken)
    {
        Guid userId = userContext.UserId;

        User? user = await dbContext.Users
            .FirstOrDefaultAsync(u => u.UserId == userId, cancellationToken);

        if (user is null)
            return UserOperationFaults.NotFound;

        Outcome<Chat> chatOutcome = Chat.Create
        (
            userId: user.UserId,
            utcNow: dateTimeProvider.UtcNow
        );

        if (chatOutcome.IsFailure)
            return chatOutcome.Fault;

        Chat chat = chatOutcome.Value;

        string title = await chatCompletionService.GetTitleAsync(request.Message, cancellationToken);

        chat.AddTitle(title, dateTimeProvider.UtcNow);

        await dbContext.Chats.AddAsync(chat, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);

        StartChatResponse response = new
        (
            ChatId: chat.Id.Value,
            ChatTitle: title,
            CreatedAt: chat.CreatedAt
        );

        return response;
    }
}
