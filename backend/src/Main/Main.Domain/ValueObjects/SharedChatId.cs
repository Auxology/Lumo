using SharedKernel;

namespace Main.Domain.ValueObjects;

public readonly record struct SharedChatId
{
    private const string Prefix = "sht_";
    private const int TotalLength = 30;

    public string Value { get; }

    private SharedChatId(string value)
    {
        Value = value;
    }

    public static Outcome<SharedChatId> From(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return Faults.Required;

        if (!IsValid(value!))
            return Faults.InvalidFormat;

        return new SharedChatId(value);
    }

    public static SharedChatId UnsafeFrom(string value) => new(value);

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
            title: "SharedChatId.Required",
            detail: "SharedChatId is required."
        );

        public static readonly Fault InvalidFormat = Fault.Validation
        (
            title: "SharedChatId.InvalidFormat",
            detail: $"SharedChatId must start with '{Prefix}' and be {TotalLength} characters."
        );
    }
}