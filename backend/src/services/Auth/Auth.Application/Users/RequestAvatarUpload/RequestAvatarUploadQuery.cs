using SharedKernel.Application.Messaging;

namespace Auth.Application.Users.RequestAvatarUpload;

public sealed record RequestAvatarUploadQuery
(
    string ContentType,
    long ContentLength
) : IQuery<RequestAvatarUploadQuery>;