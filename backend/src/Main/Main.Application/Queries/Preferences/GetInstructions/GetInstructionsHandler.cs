using System.Data.Common;

using Dapper;

using SharedKernel;
using SharedKernel.Application.Authentication;
using SharedKernel.Application.Data;
using SharedKernel.Application.Messaging;

namespace Main.Application.Queries.Preferences.GetInstructions;

internal sealed class GetInstructionsHandler(IDbConnectionFactory dbConnectionFactory, IUserContext userContext)
    : IQueryHandler<GetInstructionsQuery, GetInstructionsResponse>
{
    private const string Sql = """
                               SELECT
                                    i.id as Id,
                                    i.preference_id as PreferenceId,
                                    i.content as Content,
                                    i.priority as Priority,
                                    i.created_at as CreatedAt,
                                    i.updated_at as UpdatedAt
                               FROM instructions i
                               INNER JOIN preferences p ON p.id = i.preference_id
                               WHERE p.user_id = @UserId
                               ORDER BY i.priority ASC
                               """;

    public async ValueTask<Outcome<GetInstructionsResponse>> Handle(GetInstructionsQuery request, CancellationToken cancellationToken)
    {
        Guid userId = userContext.UserId;

        await using DbConnection connection = await dbConnectionFactory.CreateConnectionAsync(cancellationToken);

        IEnumerable<InstructionReadModel> instructions = await connection.QueryAsync<InstructionReadModel>(
            Sql,
            new { UserId = userId });

        List<InstructionReadModel> instructionList = instructions.AsList();

        GetInstructionsResponse response = new(instructionList);

        return response;
    }
}