using SharedKernel;

namespace Main.Domain.ValueObjects;

public readonly record struct ChatId
{
    public Guid Value { get; }

    private ChatId(Guid value)
    {
        Value = value;
    }

    public static ChatId New() => new(Guid.NewGuid());

    public static Outcome<ChatId> FromGuid(Guid value)
    {
        if (value == Guid.Empty)
            return Faults.Invalid;

        return new ChatId(value);
    }

    public static ChatId UnsafeFromGuid(Guid value) => new(value);

    public static Outcome<ChatId> FromString(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return Faults.StringRequired;

        if (!Guid.TryParse(value, out var guid))
            return Faults.InvalidFormat;

        return FromGuid(guid);
    }

    public override string ToString() => Value.ToString();

    public bool IsEmpty => Value == Guid.Empty;

    public static implicit operator Guid(ChatId chatId) => chatId.Value;

    public static explicit operator ChatId(Guid value) => UnsafeFromGuid(value);

    private static class Faults
    {
        public static readonly Fault Invalid = Fault.Validation
        (
            "ChatId.Invalid",
            "ChatId cannot be an empty GUID."
        );

        public static readonly Fault InvalidFormat = Fault.Validation
        (
            "ChatId.InvalidFormat",
            "The provided string is not a valid GUID format.");

        public static readonly Fault StringRequired = Fault.Validation
        (
            "ChatId.StringRequired",
            "ChatId requires a non-empty string."
        );
    }
}
