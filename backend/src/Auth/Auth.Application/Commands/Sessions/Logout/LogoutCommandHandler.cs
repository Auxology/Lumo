using Auth.Application.Abstractions.Authentication;
using Auth.Application.Abstractions.Data;
using Auth.Domain.Aggregates;
using Auth.Domain.Faults;

using Microsoft.EntityFrameworkCore;

using SharedKernel;
using SharedKernel.Application.Messaging;

namespace Auth.Application.Commands.Sessions.Logout;

internal sealed class LogoutCommandHandler(
    IAuthDbContext dbContext,
    ISecureTokenGenerator secureTokenGenerator,
    IDateTimeProvider dateTimeProvider) : ICommandHandler<LogoutCommand>
{
    public async ValueTask<Outcome> Handle(LogoutCommand request, CancellationToken cancellationToken)
    {
        if (!secureTokenGenerator.TryParseCompoundToken(request.RefreshToken, out string refreshTokenKey, out string refreshToken))
            return SessionFaults.RefreshTokenInvalidOrExpired;

        Session? session = await dbContext.Sessions
            .FirstOrDefaultAsync(s => s.RefreshTokenKey == refreshTokenKey, cancellationToken);

        if (session is null)
            return SessionFaults.RefreshTokenInvalidOrExpired;

        bool isValid = secureTokenGenerator.VerifyToken(refreshToken, session.RefreshTokenHash);

        if (!isValid)
            return SessionFaults.RefreshTokenInvalidOrExpired;

        Outcome revokeOutcome = session.RevokeDueToLogout(dateTimeProvider.UtcNow);

        if (revokeOutcome.IsFailure)
            return revokeOutcome.Fault;

        await dbContext.SaveChangesAsync(cancellationToken);

        return Outcome.Success();
    }
}