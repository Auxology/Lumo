namespace Auth.Application.Commands.Users.CancelDeletion;

public sealed record CancelUserDeletionResponse
(
    Guid Id,
    DateTimeOffset CanceledAt
);