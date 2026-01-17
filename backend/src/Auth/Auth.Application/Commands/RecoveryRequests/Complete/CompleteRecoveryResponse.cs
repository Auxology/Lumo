namespace Auth.Application.Commands.RecoveryRequests.Complete;

public sealed record CompleteRecoveryResponse
(
    string NewEmailAddress
);