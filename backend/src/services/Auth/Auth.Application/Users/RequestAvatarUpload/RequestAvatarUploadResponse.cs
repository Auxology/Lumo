namespace Auth.Application.Users.RequestAvatarUpload;

public sealed record RequestAvatarUploadResponse
(
    Uri UploadUrl,
    string AvatarKey,
    DateTimeOffset ExpiresAt
);