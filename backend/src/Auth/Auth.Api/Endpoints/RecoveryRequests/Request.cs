namespace Auth.Api.Endpoints.RecoveryRequests;

internal sealed record Request
(
    string RecoveryKey,
    string NewEmailAddress
);