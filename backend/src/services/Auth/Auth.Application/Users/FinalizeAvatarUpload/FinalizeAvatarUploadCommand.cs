using SharedKernel.Application.Messaging;

namespace Auth.Application.Users.FinalizeAvatarUpload;

public sealed record FinalizeAvatarUploadCommand
(
    string AvatarKey
) : ICommand;