namespace Auth.Api.Endpoints.Users.CancelDeletion;

internal sealed record Response
(
    Guid Id,
    DateTimeOffset CanceledAt
);