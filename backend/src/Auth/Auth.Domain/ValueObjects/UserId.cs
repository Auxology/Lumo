using SharedKernel;

namespace Auth.Domain.ValueObjects;

public readonly record struct UserId
{
    public Guid Value { get; }

    private UserId(Guid value)
    {
        Value = value;
    }

    public static UserId New() => new(Guid.NewGuid());

    public static Outcome<UserId> FromGuid(Guid value)
    {
        if (value == Guid.Empty)
            return Faults.Invalid;

        return new UserId(value);
    }

    public static UserId UnsafeFromGuid(Guid value) => new(value);

    public static Outcome<UserId> FromString(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return Faults.StringRequired;

        if (!Guid.TryParse(value, out var guid))
            return Faults.InvalidFormat;

        return FromGuid(guid);
    }

    public override string ToString() => Value.ToString();

    public bool IsEmpty => Value == Guid.Empty;

    public static implicit operator Guid(UserId userId) => userId.Value;

    public static explicit operator UserId(Guid value) => UnsafeFromGuid(value);

    private static class Faults
    {
        public static readonly Fault Invalid = Fault.Validation
        (
            "UserId.Invalid",
            "UserId cannot be an empty GUID."
        );

        public static readonly Fault InvalidFormat = Fault.Validation
        (
            "UserId.InvalidFormat",
            "The provided string is not a valid GUID format.");

        public static readonly Fault StringRequired = Fault.Validation
        (
            "UserId.StringRequired",
            "UserId requires a non-empty string."
        );
    }
}
