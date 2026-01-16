namespace Auth.Api.Endpoints.RecoveryRequests.Create;

internal sealed record Response
(
    string TokenKey,
    int RemainingRecoveryKeys
);
