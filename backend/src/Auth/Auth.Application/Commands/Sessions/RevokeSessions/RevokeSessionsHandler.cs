using Auth.Application.Abstractions.Data;
using Auth.Domain.Aggregates;
using Auth.Domain.ValueObjects;

using Microsoft.EntityFrameworkCore;

using SharedKernel;
using SharedKernel.Application.Authentication;
using SharedKernel.Application.Messaging;

namespace Auth.Application.Commands.Sessions.RevokeSessions;

internal sealed class RevokeSessionsHandler(
    IAuthDbContext dbContext,
    IUserContext userContext,
    IDateTimeProvider dateTimeProvider) : ICommandHandler<RevokeSessionsCommand>
{
    public async ValueTask<Outcome> Handle(RevokeSessionsCommand request, CancellationToken cancellationToken)
    {
        Outcome<UserId> userIdOutcome = UserId.FromGuid(userContext.UserId);

        if (userIdOutcome.IsFailure)
            return userIdOutcome.Fault;

        UserId userId = userIdOutcome.Value;

        List<SessionId> validSessionIds = request.SessionIds
            .Select(SessionId.From)
            .Where(o => o.IsSuccess)
            .Select(o => o.Value)
            .Where(id => id.Value != userContext.SessionId)
            .ToList();

        if (validSessionIds.Count == 0)
            return Outcome.Success();

        List<Session> sessions = await dbContext.Sessions
            .Where(s => validSessionIds.Contains(s.Id) && s.UserId == userId)
            .ToListAsync(cancellationToken);

        DateTimeOffset utcNow = dateTimeProvider.UtcNow;

        foreach (Session session in sessions)
        {
            session.RevokeDueToLogout(utcNow);
        }

        await dbContext.SaveChangesAsync(cancellationToken);

        return Outcome.Success();
    }
}