using SharedKernel;

namespace Auth.Domain.ValueObjects;

public readonly record struct LoginRequestId
{
    public Guid Value { get; }

    private LoginRequestId(Guid value)
    {
        Value = value;
    }

    public static LoginRequestId New() => new(Guid.NewGuid());

    public static Outcome<LoginRequestId> FromGuid(Guid value)
    {
        if (value == Guid.Empty)
            return Faults.Invalid;

        return new LoginRequestId(value);
    }

    public static LoginRequestId UnsafeFromGuid(Guid value) => new(value);

    public static Outcome<LoginRequestId> FromString(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return Faults.StringRequired;

        if (!Guid.TryParse(value, out var guid))
            return Faults.InvalidFormat;

        return FromGuid(guid);
    }

    public override string ToString() => Value.ToString();

    public bool IsEmpty => Value == Guid.Empty;

    public static implicit operator Guid(LoginRequestId loginRequestId) => loginRequestId.Value;

    public static explicit operator LoginRequestId(Guid value) => UnsafeFromGuid(value);

    private static class Faults
    {
        public static readonly Fault Invalid = Fault.Validation
        (
            title: "LoginRequestId.Invalid",
            detail: "LoginRequestId cannot be an empty GUID."
        );

        public static readonly Fault InvalidFormat = Fault.Validation
        (
            title: "LoginRequestId.InvalidFormat",
            detail: "The provided string is not a valid GUID format."
        );

        public static readonly Fault StringRequired = Fault.Validation
        (
            title: "LoginRequestId.StringRequired",
            detail: "LoginRequestId requires a non-empty string."
        );
    }
}