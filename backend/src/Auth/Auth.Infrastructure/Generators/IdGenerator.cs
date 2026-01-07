using Auth.Application.Abstractions.Generators;
using Auth.Domain.ValueObjects;

namespace Main.Infrastructure.Generators;

internal sealed class IdGenerator : IIdGenerator
{
    public SessionId NewSessionId() =>
        SessionId.UnsafeFrom($"{SessionId.PrefixValue}{Ulid.NewUlid()}");
}