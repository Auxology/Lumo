using SharedKernel;

namespace Main.Domain.ValueObjects;

public readonly record struct PreferenceId
{
    private const string Prefix = "prf_";
    private const int TotalLength = 30;

    public string Value { get; }

    private PreferenceId(string value)
    {
        Value = value;
    }

    public static Outcome<PreferenceId> From(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return Faults.Required;

        if (!IsValid(value!))
            return Faults.InvalidFormat;

        return new PreferenceId(value);
    }

    public static PreferenceId UnsafeFrom(string value) => new(value);

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
            title: "PreferenceId.Required",
            detail: "PreferenceId is required."
        );

        public static readonly Fault InvalidFormat = Fault.Validation
        (
            title: "PreferenceId.InvalidFormat",
            detail: $"PreferenceId must start with '{Prefix}' and be {TotalLength} characters."
        );
    }
}