namespace Auth.Application.Commands.Users.RequestDeletion;

public sealed record RequestUserDeletionResponse
(
    Guid Id,
    DateTimeOffset RequestedAt,
    DateTimeOffset WillBeDeletedAt
);