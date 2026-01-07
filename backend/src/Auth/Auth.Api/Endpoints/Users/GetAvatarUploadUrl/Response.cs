namespace Auth.Api.Endpoints.Users.GetAvatarUploadUrl;

internal sealed record Response
(
    string UploadUrl,
    string AvatarKey,
    DateTimeOffset ExpiresAt
);