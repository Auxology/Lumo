using SharedKernel.Application.Messaging;

namespace Auth.Application.Commands.Users.UpdateProfile;

public sealed record UpdateProfileCommand
(
    string? NewDisplayName,
    string? NewAvatarKey
) : ICommand;