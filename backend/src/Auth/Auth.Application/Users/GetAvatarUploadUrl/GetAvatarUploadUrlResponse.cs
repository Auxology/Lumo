namespace Auth.Application.Users.GetAvatarUploadUrl;

public sealed record GetAvatarUploadUrlResponse
(
    string UploadUrl,
    string AvatarKey,
    DateTimeOffset ExpiresAt
);
