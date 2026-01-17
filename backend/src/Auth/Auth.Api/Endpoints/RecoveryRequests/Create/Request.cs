namespace Auth.Api.Endpoints.RecoveryRequests.Create;

internal sealed record Request
(
    string RecoveryKey,
    string NewEmailAddress
);