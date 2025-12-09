using SharedKernel.Application.Messaging;

namespace Auth.Application.Users.RequestAvatarUpload;

public sealed record RequestAvatarUploadCommand
(
    string ContentType,
    long ContentLength
) : ICommand<RequestAvatarUploadResponse>;