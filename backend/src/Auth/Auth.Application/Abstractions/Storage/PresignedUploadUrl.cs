namespace Auth.Application.Abstractions.Storage;

public sealed record PresignedUploadUrl
(
    string Url,
    DateTimeOffset ExpiresAt
);
