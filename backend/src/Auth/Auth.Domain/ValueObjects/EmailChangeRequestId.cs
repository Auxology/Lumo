using SharedKernel;

namespace Auth.Domain.ValueObjects;

public readonly record struct EmailChangeRequestId
{
    private const string Prefix = "ecr_";
    private const int TotalLength = 30; // ecr_ (4) + 26 chars

    public string Value { get; }

    private EmailChangeRequestId(string value)
    {
        Value = value;
    }

    public static Outcome<EmailChangeRequestId> From(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return Faults.Required;

        if (!IsValid(value!))
            return Faults.InvalidFormat;

        return new EmailChangeRequestId(value);
    }

    public static EmailChangeRequestId UnsafeFrom(string value) => new(value);

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
            title: "EmailChangeRequestId.Required",
            detail: "EmailChangeRequestId is required."
        );

        public static readonly Fault InvalidFormat = Fault.Validation
        (
            title: "EmailChangeRequestId.InvalidFormat",
            detail: $"EmailChangeRequestId must start with '{Prefix}' and be {TotalLength} characters."
        );
    }
}