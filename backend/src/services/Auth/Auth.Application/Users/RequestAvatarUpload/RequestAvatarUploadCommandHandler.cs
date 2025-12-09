using Auth.Application.Abstractions.Data;
using Auth.Application.Abstractions.Storage;
using Auth.Application.Errors;
using Auth.Domain.Aggregates.UserAggregate;
using Auth.Domain.Constants;
using Auth.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using SharedKernel.Application.Authentication;
using SharedKernel.Application.Messaging;
using SharedKernel.ResultPattern;

namespace Auth.Application.Users.RequestAvatarUpload;

internal sealed class RequestAvatarUploadCommandHandler(
    IAuthDbContext dbContext,
    IUserContext userContext,
    IStorageService storageService) : ICommandHandler<RequestAvatarUploadCommand, RequestAvatarUploadResponse>
{
    public async Task<Result<RequestAvatarUploadResponse>> Handle(RequestAvatarUploadCommand request, CancellationToken cancellationToken)
    {
        Result<UserId> userIdResult = UserId.FromGuid(userContext.UserId);

        if (userIdResult.IsFailure)
            return userIdResult.Error;
        
        UserId userId = userIdResult.Value;
        
        User? user = await dbContext.Users
            .FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);

        if (user is null)
            return UserOperationErrors.UserNotFound;

        string avatarKey = storageService.GenerateFileKey(userId.Value);
        
        PresignedUploadUrl uploadUrl = await storageService.GeneratePresignedUploadUrlAsync
        (
            fileKey: avatarKey,
            contentType: request.ContentType,
            contentLength: request.ContentLength,
            expiresAt: DateTimeOffset.UtcNow.AddMinutes(UserConstants.AvatarPresignedUrlExpirationMinutes),
            cancellationToken: cancellationToken
        );
        
        RequestAvatarUploadResponse response = new
        (
            UploadUrl: uploadUrl.Url,
            AvatarKey: avatarKey
        );
        
        return response;
    }
}