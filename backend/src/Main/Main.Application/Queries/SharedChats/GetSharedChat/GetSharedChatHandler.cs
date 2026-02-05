using System.Data.Common;

using Dapper;

using Main.Application.Faults;
using Main.Domain.Faults;
using Main.Domain.ValueObjects;

using Mediator;

using SharedKernel;
using SharedKernel.Application.Data;

namespace Main.Application.Queries.SharedChats.GetSharedChat;

internal sealed class GetSharedChatHandler(IDbConnectionFactory dbConnectionFactory, IPublisher publisher)
    : SharedKernel.Application.Messaging.IQueryHandler<GetSharedChatQuery, GetSharedChatResponse>
{
    private const string SharedChatSql = """
                                         SELECT
                                             id as Id,
                                             source_chat_id as SourceChatId,
                                             owner_id as OwnerId,
                                             title as Title,
                                             model_id as ModelId,
                                             view_count as ViewCount,
                                             snapshot_at as SnapshotAt,
                                             created_at as CreatedAt
                                         FROM shared_chats
                                         WHERE id = @SharedChatId
                                         """;

    private const string MessagesSql = """
                                       SELECT
                                            sequence_number as SequenceNumber,
                                            message_role as MessageRole,
                                            message_content as MessageContent,
                                            created_at as CreatedAt,
                                            edited_at as EditedAt
                                       FROM shared_chat_messages
                                       WHERE shared_chat_id = @SharedChatId
                                       ORDER BY sequence_number ASC
                                       """;

    public async ValueTask<Outcome<GetSharedChatResponse>> Handle(GetSharedChatQuery request,
        CancellationToken cancellationToken)
    {
        Outcome<SharedChatId> sharedChatIdOutcome = SharedChatId.From(request.SharedChatId);

        if (sharedChatIdOutcome.IsFailure)
            return sharedChatIdOutcome.Fault;

        SharedChatId sharedChatId = sharedChatIdOutcome.Value;

        await using DbConnection connection = await dbConnectionFactory.CreateConnectionAsync(cancellationToken);

        SharedChatReadModel? sharedChat = await connection.QuerySingleOrDefaultAsync<SharedChatReadModel>
        (
            SharedChatSql,
            new { SharedChatId = sharedChatId.Value }
        );

        if (sharedChat is null)
            return SharedChatOperationFaults.NotFound;

        IEnumerable<SharedChatMessageReadModel> messages = await connection.QueryAsync<SharedChatMessageReadModel>
        (
            MessagesSql,
            new { SharedChatId = sharedChatId.Value }
        );

        GetSharedChatResponse response = new
        (
            SharedChat: sharedChat,
            Messages: messages.AsList()
        );

        await publisher.Publish(new SharedChatViewedNotification(request.SharedChatId), cancellationToken);

        return response;
    }
}