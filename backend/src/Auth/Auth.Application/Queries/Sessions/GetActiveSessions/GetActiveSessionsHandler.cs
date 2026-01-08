using System.Data.Common;

using Auth.Domain.ValueObjects;

using Dapper;

using SharedKernel;
using SharedKernel.Application.Authentication;
using SharedKernel.Application.Data;
using SharedKernel.Application.Messaging;

namespace Auth.Application.Queries.Sessions.GetActiveSessions;

internal sealed class GetActiveSessionsHandler(
    IDbConnectionFactory dbConnectionFactory,
    IUserContext userContext,
    IDateTimeProvider dateTimeProvider) : IQueryHandler<GetActiveSessionsQuery, IReadOnlyList<ActiveSessionReadModel>>
{
    private const string Sql = """
                               SELECT
                                    id as Id,
                                    created_at as CreatedAt,
                                    expires_at as ExpiresAt,
                                    last_refreshed_at as LastRefreshAt,
                                    fingerprint_ip_address as IpAddress,
                                    fingerprint_normalized_browser as NormalizedBrowser,
                                    fingerprint_normalized_os as NormalizedOs,
                                    (id = @CurrentSessionId) as IsCurrent
                               From sessions
                               WHERE user_id = @UserId
                                    AND revoked_at is NULL
                                    and expires_at > @UtcNow
                               ORDER BY
                                    (id = @CurrentSessionId) DESC,
                                    last_refreshed_at DESC,
                                    created_at DESC
                               """;

    public async ValueTask<Outcome<IReadOnlyList<ActiveSessionReadModel>>> Handle(GetActiveSessionsQuery request, CancellationToken cancellationToken)
    {
        Outcome<UserId> userIdOutcome = UserId.FromGuid(userContext.UserId);

        if (userIdOutcome.IsFailure)
            return userIdOutcome.Fault;

        UserId userId = userIdOutcome.Value;

        Outcome<SessionId> sessionIdOutcome = SessionId.From(userContext.SessionId);

        if (sessionIdOutcome.IsFailure)
            return sessionIdOutcome.Fault;

        SessionId sessionId = sessionIdOutcome.Value;

        await using DbConnection connection = await dbConnectionFactory.CreateConnectionAsync(cancellationToken);

        IEnumerable<ActiveSessionReadModel> sessions = await connection.QueryAsync<ActiveSessionReadModel>
        (
            Sql,
            new { UserId = userId.Value, CurrentSessionId = sessionId.Value, UtcNow = dateTimeProvider.UtcNow }
        );

        return sessions.ToList();
    }
}