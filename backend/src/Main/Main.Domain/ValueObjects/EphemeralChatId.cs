using SharedKernel;

namespace Main.Domain.ValueObjects;

public readonly record struct EphemeralChatId
{
    private const string Prefix = "ech_";
    private const int TotalLength = 30;

    public string Value { get; }

    private EphemeralChatId(string value)
    {
        Value = value;
    }

    public static Outcome<EphemeralChatId> From(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return Faults.Required;

        if (!IsValid(value!))
            return Faults.InvalidFormat;

        return new EphemeralChatId(value);
    }

    public static EphemeralChatId UnsafeFrom(string value) => new(value);

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
            title: "EphemeralChatId.Required",
            detail: "EphemeralChatId is required."
        );

        public static readonly Fault InvalidFormat = Fault.Validation
        (
            title: "EphemeralChatId.InvalidFormat",
            detail: $"EphemeralChatId must start with '{Prefix}' and be {TotalLength} characters."
        );
    }
}