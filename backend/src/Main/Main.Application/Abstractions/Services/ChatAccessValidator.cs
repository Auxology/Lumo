using System.Data.Common;

using Dapper;

using Main.Application.Faults;
using Main.Domain.ValueObjects;

using Microsoft.Extensions.Logging;

using SharedKernel;
using SharedKernel.Application.Authentication;
using SharedKernel.Application.Data;

namespace Main.Application.Abstractions.Services;

internal sealed class ChatAccessValidator(
    IDbConnectionFactory dbConnectionFactory,
    IUserContext userContext,
    ILogger<ChatAccessValidator> logger) : IChatAccessValidator
{
    private const string Sql = """
                               SELECT EXISTS
                               (
                                SELECT 1 FROM chats
                                WHERE id = @ChatId AND user_id = @UserId
                               )
                               """;

    public async Task<Outcome> ValidateAccessAsync(string chatId, CancellationToken cancellationToken)
    {
        if (logger.IsEnabled(LogLevel.Debug))
            logger.LogDebug("Validating access to chat {ChatId} for user {UserId}", chatId, userContext.UserId);

        Guid userId = userContext.UserId;
        Outcome<ChatId> chatIdOutcome = ChatId.From(chatId);

        if (chatIdOutcome.IsFailure)
            return chatIdOutcome.Fault;

        ChatId validChatId = chatIdOutcome.Value;

        await using DbConnection connection = await dbConnectionFactory.CreateConnectionAsync(cancellationToken);

        bool hasAccess = await connection.QueryFirstAsync<bool>
        (
            Sql,
            new { ChatId = validChatId.Value, UserId = userId }
        );

        if (!hasAccess)
            return ChatOperationFaults.NotFound;

        if (logger.IsEnabled(LogLevel.Debug))
            logger.LogDebug("Access to chat {ChatId} for user {UserId} validated successfully", chatId, userContext.UserId);

        return Outcome.Success();
    }
}