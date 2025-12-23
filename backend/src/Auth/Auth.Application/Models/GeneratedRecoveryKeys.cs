using Auth.Domain.ValueObjects;

namespace Auth.Application.Models;

public sealed record GeneratedRecoveryKeys
(
    IReadOnlyCollection<RecoverKeyInput> RecoverKeyInputs,
    IReadOnlyCollection<string> UserFriendlyRecoveryKeys
);