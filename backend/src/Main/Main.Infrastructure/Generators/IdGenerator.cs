using Main.Application.Abstractions.Generators;
using Main.Domain.ValueObjects;

namespace Main.Infrastructure.Generators;

internal sealed class IdGenerator : IIdGenerator
{
    public ChatId NewChatId() =>
        ChatId.UnsafeFrom($"{ChatId.PrefixValue}{Ulid.NewUlid()}");
}