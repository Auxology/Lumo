using SharedKernel;

namespace Main.Domain.ValueObjects;

public readonly record struct StreamId
{
    private const string Prefix = "str_";
    private const int TotalLength = 30;

    public string Value { get; }

    private StreamId(string value)
    {
        Value = value;
    }

    public static Outcome<StreamId> From(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return Faults.Required;

        if (!IsValid(value!))
            return Faults.InvalidFormat;

        return new StreamId(value);
    }

    public static StreamId UnsafeFrom(string value) => new(value);

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
            title: "StreamId.Required",
            detail: "StreamId is required."
        );

        public static readonly Fault InvalidFormat = Fault.Validation
        (
            title: "StreamId.InvalidFormat",
            detail: $"StreamId must start with '{Prefix}' and be {TotalLength} characters."
        );
    }
}