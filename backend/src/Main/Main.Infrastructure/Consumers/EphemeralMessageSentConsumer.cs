using Contracts.IntegrationEvents.EphemeralChat;

using Main.Application.Abstractions.AI;
using Main.Application.Abstractions.Ephemeral;
using Main.Domain.Models;
using Main.Domain.ValueObjects;

using MassTransit;

using Microsoft.Extensions.Logging;

using SharedKernel;

namespace Main.Infrastructure.Consumers;

internal sealed class EphemeralMessageSentConsumer(
    IEphemeralChatStore ephemeralChatStore,
    INativeChatCompletionService nativeChatCompletionService,
    ILogger<EphemeralMessageSentConsumer> logger) : IConsumer<EphemeralMessageSent>
{
    public async Task Consume(ConsumeContext<EphemeralMessageSent> context)
    {
        CancellationToken cancellationToken = context.CancellationToken;
        EphemeralMessageSent message = context.Message;

        string ephemeralChatId = message.EphemeralChatId;

        Outcome<StreamId> streamIdOutcome = StreamId.From(message.StreamId);

        if (streamIdOutcome.IsFailure)
        {
            logger.LogError(
                "Invalid StreamId in {EventType}: {EventId}, CorrelationId: {CorrelationId}, StreamId: {StreamId}",
                nameof(EphemeralMessageSent), message.EventId, message.CorrelationId, message.StreamId);
            return;
        }

        StreamId streamId = streamIdOutcome.Value;

        EphemeralChat? ephemeralChat = await ephemeralChatStore.GetAsync(ephemeralChatId, cancellationToken);

        if (ephemeralChat is null)
        {
            logger.LogError(
                "Ephemeral chat not found for {EventType}: {EventId}, CorrelationId: {CorrelationId}, EphemeralChatId: {EphemeralChatId}",
                nameof(EphemeralMessageSent), message.EventId, message.CorrelationId, ephemeralChatId);
            return;
        }

        List<ChatCompletionMessage> messages = ephemeralChat.Messages
            .OrderBy(m => m.SequenceNumber)
            .Select(m => new ChatCompletionMessage
            (
                Role: m.MessageRole,
                Content: m.MessageContent
            ))
            .ToList();

        await nativeChatCompletionService.StreamCompletionAsync
        (
            chatId: ephemeralChatId,
            streamId: streamId.Value,
            modelId: message.ModelId,
            messages: messages,
            correlationId: message.CorrelationId.ToString(),
            cancellationToken: cancellationToken
        );

        if (logger.IsEnabled(LogLevel.Information))
            logger.LogInformation(
                "Consumed {EventType}: {EventId}, CorrelationId: {CorrelationId}, OccurredAt: {OccurredAt}, EphemeralChatId: {EphemeralChatId}",
                nameof(EphemeralMessageSent), message.EventId, message.CorrelationId, message.OccurredAt, ephemeralChatId);
    }
}