namespace Auth.Api.Endpoints.Users.RequestDeletion;

internal sealed record Response
(
    Guid Id,
    DateTimeOffset RequestedAt,
    DateTimeOffset WillBeDeletedAt
);