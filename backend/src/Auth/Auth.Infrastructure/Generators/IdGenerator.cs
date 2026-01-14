using Auth.Application.Abstractions.Generators;
using Auth.Domain.ValueObjects;

namespace Auth.Infrastructure.Generators;

internal sealed class IdGenerator : IIdGenerator
{
    public SessionId NewSessionId() =>
        SessionId.UnsafeFrom($"{SessionId.PrefixValue}{Ulid.NewUlid()}");

    public LoginRequestId NewLoginRequestId() =>
        LoginRequestId.UnsafeFrom($"{LoginRequestId.PrefixValue}{Ulid.NewUlid()}");

    public RecoveryKeyChainId NewRecoveryKeyChainId() =>
        RecoveryKeyChainId.UnsafeFrom($"{RecoveryKeyChainId.PrefixValue}{Ulid.NewUlid()}");

    public RecoveryRequestId NewRecoveryRequestId() =>
        RecoveryRequestId.UnsafeFrom($"{RecoveryRequestId.PrefixValue}{Ulid.NewUlid()}");
}