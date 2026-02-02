using System.Data.Common;

using Dapper;

using Main.Domain.ValueObjects;

using SharedKernel;
using SharedKernel.Application.Authentication;
using SharedKernel.Application.Data;
using SharedKernel.Application.Messaging;

namespace Main.Application.Queries.SharedChats.GetSharedChatsByOriginal;

internal sealed class GetSharedChatByOriginalHandler(IDbConnectionFactory dbConnectionFactory, IUserContext userContext)
    : IQueryHandler<GetSharedChatByOriginalQuery, GetSharedChatByOriginalResponse>
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
                                         WHERE source_chat_id = @SourceChatId AND owner_id = @UserId
                                         """;

    public async ValueTask<Outcome<GetSharedChatByOriginalResponse>> Handle(GetSharedChatByOriginalQuery request, CancellationToken cancellationToken)
    {
        Outcome<ChatId> chatIdOutcome = ChatId.From(request.OriginalChatId);

        if (chatIdOutcome.IsFailure)
            return chatIdOutcome.Fault;

        ChatId chatId = chatIdOutcome.Value;
        Guid userId = userContext.UserId;

        await using DbConnection connection = await dbConnectionFactory.CreateConnectionAsync(cancellationToken);

        IEnumerable<SharedChatSummaryReadModel> sharedChats = await connection.QueryAsync<SharedChatSummaryReadModel>
        (
            SharedChatSql,
            new { SourceChatId = chatId.Value, UserId = userId }
        );

        GetSharedChatByOriginalResponse response = new(sharedChats.AsList());

        return response;
    }
}