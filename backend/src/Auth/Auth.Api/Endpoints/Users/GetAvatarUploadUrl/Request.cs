namespace Auth.Api.Endpoints.Users.GetAvatarUploadUrl;

internal sealed record Request
(
    string ContentType,
    long ContentLength
);