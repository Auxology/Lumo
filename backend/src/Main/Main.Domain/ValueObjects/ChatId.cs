using SharedKernel;

namespace Main.Domain.ValueObjects;

public readonly record struct ChatId
{
    private const string Prefix = "cht_";
    private const int TotalLength = 30;

    public string Value { get; }

    private ChatId(string value)
    {
        Value = value;
    }

    public static Outcome<ChatId> From(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return Faults.Required;

        if (!IsValid(value!))
            return Faults.InvalidFormat;

        return new ChatId(value);
    }

    public static ChatId UnsafeFrom(string value) => new(value);

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
            title: "ChatId.Required",
            detail: "ChatId is required."
        );

        public static readonly Fault InvalidFormat = Fault.Validation
        (
            title: "ChatId.InvalidFormat",
            detail: $"ChatId must start with '{Prefix}' and be {TotalLength} characters."
        );
    }
}