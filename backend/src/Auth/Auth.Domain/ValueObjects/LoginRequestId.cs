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
            return Outcome.Failure<LoginRequestId>(Errors.Invalid);

        return Outcome.Success(new LoginRequestId(value));
    }

    public static LoginRequestId UnsafeFromGuid(Guid value) => new(value);

    public static Outcome<LoginRequestId> FromString(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return Outcome.Failure<LoginRequestId>(Errors.StringRequired);

        if (!Guid.TryParse(value, out var guid))
            return Outcome.Failure<LoginRequestId>(Errors.InvalidFormat);

        return FromGuid(guid);
    }

    public override string ToString() => Value.ToString();

    public bool IsEmpty => Value == Guid.Empty;

    public static implicit operator Guid(LoginRequestId loginRequestId) => loginRequestId.Value;

    public static explicit operator LoginRequestId(Guid value) => UnsafeFromGuid(value);

    private static class Errors
    {
        public static readonly Fault Invalid = Fault.Validation
        (
            "LoginRequestId.Invalid",
            "LoginRequestId cannot be an empty GUID.");

        public static readonly Fault InvalidFormat = Fault.Validation
        (
            "LoginRequestId.InvalidFormat",
            "The provided string is not a valid GUID format.");

        public static readonly Fault StringRequired = Fault.Validation
        (
            "LoginRequestId.StringRequired",
            "LoginRequestId requires a non-empty string."
        );
    }
}