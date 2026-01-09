using Contracts.IntegrationEvents.Chat;

using Main.Application.Abstractions.AI;
using Main.Application.Abstractions.Data;
using Main.Application.Abstractions.Generators;
using Main.Application.Faults;
using Main.Domain.Aggregates;
using Main.Domain.ReadModels;
using Main.Domain.ValueObjects;

using Microsoft.EntityFrameworkCore;

using SharedKernel;
using SharedKernel.Application.Authentication;
using SharedKernel.Application.Messaging;

namespace Main.Application.Commands.Chats.Start;

internal sealed class StartChatHandler(
    IMainDbContext dbContext,
    IUserContext userContext,
    IIdGenerator idGenerator,
    IRequestContext requestContext,
    IChatCompletionService chatCompletionService,
    IMessageBus messageBus,
    IDateTimeProvider dateTimeProvider) : ICommandHandler<StartChatCommand, StartChatResponse>
{
    public async ValueTask<Outcome<StartChatResponse>> Handle(StartChatCommand request, CancellationToken cancellationToken)
    {
        Guid userId = userContext.UserId;

        User? user = await dbContext.Users
            .FirstOrDefaultAsync(u => u.UserId == userId, cancellationToken);

        if (user is null)
            return UserOperationFaults.NotFound;

        string title = await chatCompletionService.GetTitleAsync(request.Message, cancellationToken);

        ChatId chatId = idGenerator.NewChatId();

        Outcome<Chat> chatOutcome = Chat.Create
        (
            id: chatId,
            userId: user.UserId,
            title: title,
            utcNow: dateTimeProvider.UtcNow
        );

        if (chatOutcome.IsFailure)
            return chatOutcome.Fault;

        Chat chat = chatOutcome.Value;

        Outcome messageOutcome = chat.AddUserMessage
        (
            messageContent: request.Message,
            utcNow: dateTimeProvider.UtcNow
        );

        if (messageOutcome.IsFailure)
            return messageOutcome.Fault;

        ChatStarted chatStarted = new()
        {
            EventId = Guid.NewGuid(),
            OccurredAt = dateTimeProvider.UtcNow,
            CorrelationId = Guid.Parse(requestContext.CorrelationId),
            ChatId = chat.Id.Value,
            UserId = user.UserId,
            InitialMessage = request.Message
        };

        await dbContext.Chats.AddAsync(chat, cancellationToken);
        await messageBus.PublishAsync(chatStarted, cancellationToken);
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