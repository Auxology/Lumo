using Contracts.IntegrationEvents.Chat;
using Main.Application.Abstractions.AI;
using Main.Application.Abstractions.Data;
using Main.Domain.Constants;
using Main.Domain.Entities;
using Main.Domain.ValueObjects;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SharedKernel;

namespace Main.Infrastructure.Consumers;

internal sealed class ChatStartedConsumer(
    IMainDbContext dbContext,
    IChatCompletionService chatCompletionService,
    ILogger<ChatStartedConsumer> logger,
    IDateTimeProvider dateTimeProvider) : IConsumer<ChatStarted>
{
    public async Task Consume(ConsumeContext<ChatStarted> context)
    {
        CancellationToken cancellationToken = context.CancellationToken;
        Guid eventId = context.Message.EventId;

        bool alreadyProcessed = await dbContext.ProcessedEvents
            .AnyAsync(e => e.EventId == eventId, cancellationToken);

        if (alreadyProcessed)
            return;

        ProcessedEvent processedEvent = ProcessedEvent.Create(eventId, dateTimeProvider.UtcNow);

        Outcome<ChatId> chatIdOutcome = ChatId.FromGuid(context.Message.ChatId);

        if (chatIdOutcome.IsFailure)
        {
            logger.LogError("Invalid ChatId in ChatStarted event: {ChatId}", context.Message.ChatId);
            return;
        }

        ChatId chatId = chatIdOutcome.Value;

        List<ChatCompletionMessage> messages = await dbContext.Messages
            .Where(c => c.ChatId == chatId)
            .OrderByDescending(c => c.CreatedAt)
            .Take(ChatConstants.MaxContextMessages)
            .OrderBy(c => c.CreatedAt)
            .Select(m => new ChatCompletionMessage
            (
                Role: m.MessageRole,
                Content: m.MessageContent
            ))
            .ToListAsync(cancellationToken);

        await dbContext.ProcessedEvents.AddAsync(processedEvent, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);

        await chatCompletionService.StreamCompletionAsync
        (
            chatId: chatId.Value,
            messages: messages,
            cancellationToken: cancellationToken
        );

        logger.LogInformation("Processed ChatStarted event: {EventId}", eventId);
    }
}
