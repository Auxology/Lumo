using System.Data.Common;

using Auth.Application.Faults;
using Auth.Domain.ValueObjects;

using Dapper;

using SharedKernel;
using SharedKernel.Application.Authentication;
using SharedKernel.Application.Data;
using SharedKernel.Application.Messaging;

namespace Auth.Application.Queries.Users.GetCurrentUser;

internal sealed class GetCurrentUserHandler(IDbConnectionFactory dbConnectionFactory, IUserContext userContext)
    : IQueryHandler<GetCurrentUserQuery, UserReadModel>
{
    private const string Sql = """
                               SELECT
                                    id as Id,
                                    display_name as DisplayName,
                                    email_address as EmailAddress,
                                    created_at as CreatedAt
                               FROM users
                               where id = @UserId
                               """;

    public async ValueTask<Outcome<UserReadModel>> Handle(GetCurrentUserQuery request,
        CancellationToken cancellationToken)
    {
        Outcome<UserId> userIdOutcome = UserId.FromGuid(userContext.UserId);

        if (userIdOutcome.IsFailure)
            return userIdOutcome.Fault;

        UserId userId = userIdOutcome.Value;

        await using DbConnection connection = await dbConnectionFactory.CreateConnectionAsync(cancellationToken);

        UserReadModel? user = await connection.QuerySingleOrDefaultAsync<UserReadModel>
        (
            Sql,
            new { UserId = userId.Value }
        );

        if (user is null)
            return UserOperationFaults.NotFound;

        return user;
    }
}