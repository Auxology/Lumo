using SharedKernel;

namespace Auth.Domain.ValueObjects;

public readonly record struct SessionId
{
    public Guid Value { get; }

    private SessionId(Guid value)
    {
        Value = value;
    }

    public static SessionId New() => new(Guid.NewGuid());

    public static Outcome<SessionId> FromGuid(Guid value)
    {
        if (value == Guid.Empty)
            return Faults.Invalid;

        return new SessionId(value);
    }

    public static SessionId UnsafeFromGuid(Guid value) => new(value);

    public static Outcome<SessionId> FromString(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return Faults.StringRequired;

        if (!Guid.TryParse(value, out var guid))
            return Faults.InvalidFormat;

        return FromGuid(guid);
    }

    public override string ToString() => Value.ToString();

    public bool IsEmpty => Value == Guid.Empty;

    public static implicit operator Guid(SessionId sessionId) => sessionId.Value;

    public static explicit operator SessionId(Guid value) => UnsafeFromGuid(value);

    private static class Faults
    {
        public static readonly Fault Invalid = Fault.Validation
        (
            title: "SessionId.Invalid",
            detail: "SessionId cannot be an empty GUID."
        );

        public static readonly Fault InvalidFormat = Fault.Validation
        (
            title: "SessionId.InvalidFormat",
            detail: "The provided string is not a valid GUID format."
        );

        public static readonly Fault StringRequired = Fault.Validation
        (
            title: "SessionId.StringRequired",
            detail: "SessionId requires a non-empty string."
        );
    }
}