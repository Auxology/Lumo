using Contracts.IntegrationEvents.Chat;

using Main.Application.Abstractions.Data;
using Main.Application.Faults;
using Main.Domain.Aggregates;
using Main.Domain.Entities;
using Main.Domain.ReadModels;
using Main.Domain.ValueObjects;

using Microsoft.EntityFrameworkCore;

using SharedKernel;
using SharedKernel.Application.Authentication;
using SharedKernel.Application.Messaging;

namespace Main.Application.Commands.Chats.SendMessage;

internal sealed class SendMessageHandler(
    IMainDbContext dbContext,
    IUserContext userContext,
    IRequestContext requestContext,
    IMessageBus messageBus,
    IDateTimeProvider dateTimeProvider) : ICommandHandler<SendMessageCommand, SendMessageResponse>
{
    public async ValueTask<Outcome<SendMessageResponse>> Handle(SendMessageCommand request, CancellationToken cancellationToken)
    {
        Guid userId = userContext.UserId;

        User? user = await dbContext.Users
            .FirstOrDefaultAsync(u => u.UserId == userId, cancellationToken);

        if (user is null)
            return UserOperationFaults.NotFound;

        Outcome<ChatId> chatIdOutcome = ChatId.From(request.ChatId);

        if (chatIdOutcome.IsFailure)
            return chatIdOutcome.Fault;

        ChatId chatId = chatIdOutcome.Value;

        Chat? chat = await dbContext.Chats
            .FirstOrDefaultAsync(c => c.Id == chatId && c.UserId == userId, cancellationToken);

        if (chat is null)
            return ChatOperationFaults.NotFound;

        Outcome<Message> messageOutcome = chat.AddUserMessage
        (
            messageContent: request.Message,
            utcNow: dateTimeProvider.UtcNow
        );

        if (messageOutcome.IsFailure)
            return messageOutcome.Fault;

        Message message = messageOutcome.Value;

        MessageSent messageSent = new()
        {
            EventId = Guid.NewGuid(),
            OccurredAt = dateTimeProvider.UtcNow,
            CorrelationId = Guid.Parse(requestContext.CorrelationId),
            ChatId = chat.Id.Value,
            UserId = user.UserId,
            Message = request.Message
        };

        await messageBus.PublishAsync(messageSent, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);

        SendMessageResponse response = new
        (
            MessageId: message.Id,
            ChatId: chat.Id.Value,
            MessageRole: message.MessageRole.ToString(),
            MessageContent: message.MessageContent,
            CreatedAt: message.CreatedAt
        );

        return response;
    }
}