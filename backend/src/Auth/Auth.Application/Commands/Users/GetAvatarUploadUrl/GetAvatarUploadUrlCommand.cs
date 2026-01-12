using SharedKernel.Application.Messaging;

namespace Auth.Application.Commands.Users.GetAvatarUploadUrl;

public sealed record GetAvatarUploadUrlCommand
(
    string ContentType,
    long ContentLength
) : ICommand<GetAvatarUploadUrlResponse>;