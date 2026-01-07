namespace Auth.Api.Endpoints.Users.UpdateProfile;

internal sealed record Request
(
    string? NewDisplayName,
    string? NewAvatarKey
);