using Contracts.IntegrationEvents.EphemeralChat;

using Main.Application.Abstractions.AI;
using Main.Application.Abstractions.Data;
using Main.Application.Abstractions.Ephemeral;
using Main.Application.Abstractions.Generators;
using Main.Application.Abstractions.Stream;
using Main.Application.Faults;
using Main.Domain.Enums;
using Main.Domain.Models;
using Main.Domain.ReadModels;
using Main.Domain.ValueObjects;

using Microsoft.EntityFrameworkCore;

using SharedKernel;
using SharedKernel.Application.Authentication;
using SharedKernel.Application.Messaging;

namespace Main.Application.Commands.EphemeralChats.Start;

internal sealed class StartEphemeralChatHandler(
    IMainDbContext dbContext,
    IUserContext userContext,
    IRequestContext requestContext,
    IModelRegistry modelRegistry,
    IChatLockService chatLockService,
    IEphemeralChatStore ephemeralChatStore,
    IIdGenerator idGenerator,
    IMessageBus messageBus,
    IDateTimeProvider dateTimeProvider) : ICommandHandler<StartEphemeralChatCommand, StartEphemeralChatResponse>
{
    public async ValueTask<Outcome<StartEphemeralChatResponse>> Handle(StartEphemeralChatCommand request, CancellationToken cancellationToken)
    {
        Guid userId = userContext.UserId;

        User? user = await dbContext.Users
            .FirstOrDefaultAsync(u => u.UserId == userId, cancellationToken);

        if (user is null)
            return UserOperationFaults.NotFound;

        string modelId = request.ModelId ?? modelRegistry.GetDefaultModelId();

        bool isAllowed = modelRegistry.IsModelAllowed(modelId);

        if (!isAllowed)
            return ChatOperationFaults.InvalidModel;

        EphemeralChatId ephemeralChatId = idGenerator.NewEphemeralChatId();

        EphemeralMessage ephemeralMessage = new()
        {
            MessageRole = MessageRole.User,
            MessageContent = request.Message,
            SequenceNumber = 0
        };

        EphemeralChat ephemeralChat = new()
        {
            EphemeralChatId = ephemeralChatId.Value,
            ModelId = modelId,
            UserId = userId,
            Messages = [ephemeralMessage],
            CreatedAt = dateTimeProvider.UtcNow
        };

        bool lockAcquired = await chatLockService.TryAcquireLockAsync
        (
            chatId: ephemeralChatId.Value,
            ownerId: requestContext.CorrelationId,
            cancellationToken: cancellationToken
        );

        if (!lockAcquired)
            return ChatOperationFaults.GenerationInProgress;

        try
        {
            StreamId streamId = idGenerator.NewStreamId();

            EphemeralChatStarted ephemeralChatStarted = new()
            {
                EventId = Guid.NewGuid(),
                OccurredAt = dateTimeProvider.UtcNow,
                CorrelationId = Guid.Parse(requestContext.CorrelationId),
                EphemeralChatId = ephemeralChat.EphemeralChatId,
                UserId = ephemeralChat.UserId,
                StreamId = streamId.Value,
                ModelId = ephemeralChat.ModelId,
                InitialMessage = ephemeralMessage.MessageContent
            };

            await ephemeralChatStore.SaveAsync(ephemeralChat, cancellationToken);
            await messageBus.PublishAsync(ephemeralChatStarted, cancellationToken);
            await dbContext.SaveChangesAsync(cancellationToken);

            StartEphemeralChatResponse response = new
            (
                EphemeralChatId: ephemeralChatId.Value,
                StreamId: streamId.Value
            );

            return response;
        }
        catch
        {
            await chatLockService.ReleaseLockAsync
            (
                chatId: ephemeralChatId.Value,
                ownerId: requestContext.CorrelationId,
                cancellationToken
            );
            throw;
        }
    }
}