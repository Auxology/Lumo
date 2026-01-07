using SharedKernel.Application.Messaging;

namespace Auth.Application.Users.UpdateProfile;

public sealed record UpdateProfileCommand
(
    string? NewDisplayName,
    string? NewAvatarKey
) : ICommand;