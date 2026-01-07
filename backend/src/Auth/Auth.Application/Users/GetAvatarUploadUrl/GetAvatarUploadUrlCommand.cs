using SharedKernel.Application.Messaging;

namespace Auth.Application.Users.GetAvatarUploadUrl;

public sealed record GetAvatarUploadUrlCommand
(
    string ContentType,
    long ContentLength
) : ICommand<GetAvatarUploadUrlResponse>;