using Auth.Application.Abstractions.Data;
using Auth.Application.Abstractions.Storage;
using Auth.Application.Errors;
using Auth.Domain.Aggregates.UserAggregate;
using Auth.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using SharedKernel.Application.Authentication;
using SharedKernel.Application.Messaging;
using SharedKernel.ResultPattern;
using SharedKernel.Time;

namespace Auth.Application.Users.FinalizeAvatarUpload;

internal sealed class FinalizeAvatarUploadCommandHandler(
    IAuthDbContext dbContext,
    IUserContext userContext,
    IStorageService storageService,
    IDateTimeProvider dateTimeProvider) : ICommandHandler<FinalizeAvatarUploadCommand>
{
    public async Task<Result> Handle(FinalizeAvatarUploadCommand request, CancellationToken cancellationToken)
    {
        Result<UserId> userIdResult = UserId.FromGuid(userContext.UserId);

        if (userIdResult.IsFailure)
            return userIdResult.Error;
        
        UserId userId = userIdResult.Value;
        
        User? user = await dbContext.Users
            .FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);

        if (user is null)
            return UserOperationErrors.UserNotFound;
        
        bool fileExists = await storageService.FileExistsAsync(request.AvatarKey, cancellationToken);
        
        if (!fileExists)
            return UserOperationErrors.AvatarFileNotFound;
        
        bool keyBelongsToUser = storageService.ValidateKeyOwnership(request.AvatarKey, userId.Value);
        
        if (!keyBelongsToUser)
            return UserOperationErrors.AvatarKeyDoesNotBelongToUser;

        Result setAvatarResult = user.SetAvatar
        (
            avatarKey: request.AvatarKey,
            dateTimeProvider: dateTimeProvider
        );
        
        if (setAvatarResult.IsFailure)
            return setAvatarResult.Error;
        
        await dbContext.SaveChangesAsync(cancellationToken);
        
        return Result.Success();
    }
}