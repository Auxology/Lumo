using Auth.Application.Abstractions.Data;
using Auth.Application.Faults;
using Auth.Domain.Aggregates;
using Auth.Domain.ValueObjects;

using Microsoft.EntityFrameworkCore;

using SharedKernel;
using SharedKernel.Application.Authentication;
using SharedKernel.Application.Messaging;

namespace Auth.Application.Commands.EmailChangeRequests.Cancel;

internal sealed class CancelEmailChangeHandler(
    IAuthDbContext dbContext,
    IUserContext userContext,
    IDateTimeProvider dateTimeProvider
) : ICommandHandler<CancelEmailChangeCommand>
{
    public async ValueTask<Outcome> Handle(
        CancelEmailChangeCommand request,
        CancellationToken cancellationToken)
    {
        Outcome<UserId> userIdOutcome = UserId.FromGuid(userContext.UserId);

        if (userIdOutcome.IsFailure)
            return userIdOutcome.Fault;

        UserId userId = userIdOutcome.Value;

        EmailChangeRequest? emailChangeRequest = await dbContext.EmailChangeRequests
            .FirstOrDefaultAsync(ecr => ecr.TokenKey == request.TokenKey &&
                                        ecr.UserId == userId,
                cancellationToken);

        if (emailChangeRequest is null)
            return EmailChangeRequestOperationFaults.NotFoundOrNotOwned;

        Outcome cancelOutcome = emailChangeRequest.Cancel(dateTimeProvider.UtcNow);

        if (cancelOutcome.IsFailure)
            return cancelOutcome.Fault;

        await dbContext.SaveChangesAsync(cancellationToken);

        return Outcome.Success();
    }
}