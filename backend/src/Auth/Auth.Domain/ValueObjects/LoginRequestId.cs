using SharedKernel;

namespace Auth.Domain.ValueObjects;

public readonly record struct LoginRequestId
{
    private const string Prefix = "lrq_";
    private const int TotalLength = 30;

    public string Value { get; }

    private LoginRequestId(string value)
    {
        Value = value;
    }

    public static Outcome<LoginRequestId> From(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return Faults.Required;

        if (!IsValid(value!))
            return Faults.InvalidFormat;

        return new LoginRequestId(value);
    }

    public static LoginRequestId UnsafeFrom(string value) => new(value);

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
            title: "LoginRequestId.Required",
            detail: "LoginRequestId is required."
        );

        public static readonly Fault InvalidFormat = Fault.Validation
        (
            title: "LoginRequestId.InvalidFormat",
            detail: $"LoginRequestId must start with '{Prefix}' and be {TotalLength} characters."
        );
    }
}
