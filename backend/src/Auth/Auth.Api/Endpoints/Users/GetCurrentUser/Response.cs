namespace Auth.Api.Endpoints.Users.GetCurrentUser;

internal sealed record Response
(
    Guid Id,
    string DisplayName,
    string EmailAddress,
    string? AvatarUrl,
    DateTimeOffset CreatedAt
);