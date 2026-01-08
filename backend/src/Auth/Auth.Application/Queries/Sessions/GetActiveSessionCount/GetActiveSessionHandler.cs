using System.Data.Common;

using Auth.Domain.ValueObjects;

using Dapper;

using SharedKernel;
using SharedKernel.Application.Authentication;
using SharedKernel.Application.Data;
using SharedKernel.Application.Messaging;

namespace Auth.Application.Queries.Sessions.GetActiveSessionCount;

internal sealed class GetActiveSessionHandler(
    IDbConnectionFactory dbConnectionFactory,
    IUserContext userContext,
    IDateTimeProvider dateTimeProvider) : IQueryHandler<GetActiveSessionCountQuery, int>
{
    private const string Sql = """
                               SELECT COUNT(*)
                               FROM sessions
                               WHERE user_id = @UserId
                                    AND revoked_at is NULL
                                    AND expires_at > @UtcNow
                               """;

    public async ValueTask<Outcome<int>> Handle(GetActiveSessionCountQuery request, CancellationToken cancellationToken)
    {
        Outcome<UserId> userIdOutcome = UserId.FromGuid(userContext.UserId);

        if (userIdOutcome.IsFailure)
            return userIdOutcome.Fault;

        UserId userId = userIdOutcome.Value;

        await using DbConnection connection = await dbConnectionFactory.CreateConnectionAsync(cancellationToken);

        int count = await connection.ExecuteScalarAsync<int>
        (
            Sql,
            new { UserId = userId.Value, UtcNow = dateTimeProvider.UtcNow }
        );

        return count;
    }
}