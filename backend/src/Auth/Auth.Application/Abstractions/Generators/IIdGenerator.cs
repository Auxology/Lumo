using Auth.Domain.ValueObjects;

namespace Auth.Application.Abstractions.Generators;

public interface IIdGenerator
{
    SessionId NewSessionId();

    LoginRequestId NewLoginRequestId();

    RecoveryKeyChainId NewRecoveryKeyChainId();
}