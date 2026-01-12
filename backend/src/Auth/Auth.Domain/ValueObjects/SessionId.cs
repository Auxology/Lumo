using SharedKernel;

namespace Auth.Domain.ValueObjects;

public readonly record struct SessionId
{
    private const string Prefix = "sid_";
    private const int TotalLength = 30;

    public string Value { get; }

    private SessionId(string value)
    {
        Value = value;
    }

    public static Outcome<SessionId> From(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return Faults.Required;

        if (!IsValid(value!))
            return Faults.InvalidFormat;

        return new SessionId(value);
    }

    public static SessionId UnsafeFrom(string value) => new(value);

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
            title: "SessionId.Required",
            detail: "SessionId is required."
        );

        public static readonly Fault InvalidFormat = Fault.Validation
        (
            title: "SessionId.InvalidFormat",
            detail: $"SessionId must start with '{Prefix}' and be {TotalLength} characters."
        );
    }
}