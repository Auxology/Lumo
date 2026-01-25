using Main.Application.Abstractions.Generators;
using Main.Domain.ValueObjects;

namespace Main.Infrastructure.Generators;

internal sealed class IdGenerator : IIdGenerator
{
    public ChatId NewChatId() =>
        ChatId.UnsafeFrom($"{ChatId.PrefixValue}{Ulid.NewUlid()}");

    public MessageId NewMessageId() =>
        MessageId.UnsafeFrom($"{MessageId.PrefixValue}{Ulid.NewUlid()}");

    public StreamId NewStreamId() =>
        StreamId.UnsafeFrom($"{StreamId.PrefixValue}{Ulid.NewUlid()}");

    public PreferenceId NewPreferenceId() =>
        PreferenceId.UnsafeFrom($"{PreferenceId.PrefixValue}{Ulid.NewUlid()}");

    public InstructionId NewInstructionId() =>
        InstructionId.UnsafeFrom($"{InstructionId.PrefixValue}{Ulid.NewUlid()}");
}