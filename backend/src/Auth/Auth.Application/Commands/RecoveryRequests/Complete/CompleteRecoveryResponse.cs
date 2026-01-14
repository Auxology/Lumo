namespace Auth.Application.Commands.RecoveryRequests.Complete;

internal sealed record CompleteRecoveryResponse
(
    string NewEmailAddress
);