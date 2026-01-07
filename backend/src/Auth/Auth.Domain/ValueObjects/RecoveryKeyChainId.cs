using SharedKernel;

namespace Auth.Domain.ValueObjects;

public readonly record struct RecoveryKeyChainId
{
    private const string Prefix = "rkc_";
    private const int TotalLength = 30;

    public string Value { get; }

    private RecoveryKeyChainId(string value)
    {
        Value = value;
    }

    public static Outcome<RecoveryKeyChainId> From(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return Faults.Required;

        if (!IsValid(value!))
            return Faults.InvalidFormat;

        return new RecoveryKeyChainId(value);
    }

    public static RecoveryKeyChainId UnsafeFrom(string value) => new(value);

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
            title: "RecoveryKeyChainId.Required",
            detail: "RecoveryKeyChainId is required."
        );

        public static readonly Fault InvalidFormat = Fault.Validation
        (
            title: "RecoveryKeyChainId.InvalidFormat",
            detail: $"RecoveryKeyChainId must start with '{Prefix}' and be {TotalLength} characters."
        );
    }
}
