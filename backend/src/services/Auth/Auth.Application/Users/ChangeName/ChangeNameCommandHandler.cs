using Auth.Application.Abstractions.Data;
using Auth.Application.Errors;
using Auth.Domain.Aggregates.UserAggregate;
using Auth.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using SharedKernel.Application.Authentication;
using SharedKernel.Application.Messaging;
using SharedKernel.ResultPattern;
using SharedKernel.Time;

namespace Auth.Application.Users.ChangeName;

internal sealed class ChangeNameCommandHandler(IAuthDbContext dbContext, IUserContext userContext, IDateTimeProvider dateTimeProvider) : ICommandHandler<ChangeNameCommand>
{
    public async Task<Result> Handle(ChangeNameCommand request, CancellationToken cancellationToken)
    {
        Result<UserId> userIdResult = UserId.FromGuid(userContext.UserId);

        if (userIdResult.IsFailure)
            return userIdResult.Error;
        
        UserId userId = userIdResult.Value;
        
        User? user = await dbContext.Users
            .FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);

        if (user is null)
            return UserOperationErrors.UserNotFound;

        Result changeNameResult = user.ChangeDisplayName
        (
            newDisplayName: request.NewDisplayName,
            dateTimeProvider: dateTimeProvider
        );
        
        if (changeNameResult.IsFailure)
            return changeNameResult.Error;
        
        await dbContext.SaveChangesAsync(cancellationToken);
        
        return Result.Success();
    }
}