using SharedKernel;

namespace Main.Domain.ValueObjects;

public readonly record struct MessageId
{
    private const string Prefix = "msg_";
    private const int TotalLength = 30;

    public string Value { get; }

    private MessageId(string value)
    {
        Value = value;
    }

    public static Outcome<MessageId> From(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return Faults.Required;

        if (!IsValid(value!))
            return Faults.InvalidFormat;

        return new MessageId(value);
    }

    public static MessageId UnsafeFrom(string value) => new(value);

    public static string PrefixValue => Prefix;

    public static int Length => TotalLength;

    private static bool IsValid(string value) =>
        value.Length == TotalLength && value.StartsWith(Prefix, StringComparison.Ordinal);

    public override string ToString() => Value;

    public bool IsEmpty => string.IsNullOrEmpty(Value);

    private static class Faults
    {
        public static readonly Fault Required = Fault.Validation
        (
            title: "MessageId.Required",
            detail: "MessageId is required."
        );

        public static readonly Fault InvalidFormat = Fault.Validation
        (
            title: "MessageId.InvalidFormat",
            detail: $"MessageId must start with '{Prefix}' and be {TotalLength} characters."
        );
    }
}