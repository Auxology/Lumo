namespace Auth.Api.Endpoints.RecoveryRequests;

internal sealed record Response
(
    string TokenKey,
    int RemainingRecoveryKeys
);