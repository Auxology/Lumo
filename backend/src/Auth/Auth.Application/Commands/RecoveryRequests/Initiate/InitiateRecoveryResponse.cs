namespace Auth.Application.Commands.RecoveryRequests.Initiate;

public sealed record InitiateRecoveryResponse
(
    string TokenKey,
    int RemainingRecoveryKeys
);