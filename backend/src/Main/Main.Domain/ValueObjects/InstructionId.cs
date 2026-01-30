using SharedKernel;

namespace Main.Domain.ValueObjects;

public readonly record struct InstructionId
{
    private const string Prefix = "ins_";
    private const int TotalLength = 30;

    public string Value { get; }

    private InstructionId(string value)
    {
        Value = value;
    }

    public static Outcome<InstructionId> From(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return Faults.Required;

        if (!IsValid(value!))
            return Faults.InvalidFormat;

        return new InstructionId(value);
    }

    public static InstructionId UnsafeFrom(string value) => new(value);

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
            title: "InstructionId.Required",
            detail: "InstructionId is required."
        );

        public static readonly Fault InvalidFormat = Fault.Validation
        (
            title: "InstructionId.InvalidFormat",
            detail: $"InstructionId must start with '{Prefix}' and be {TotalLength} characters."
        );
    }
}