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
            return Outcome.Failure<SessionId>(Errors.Invalid);

        return Outcome.Success(new SessionId(value));
    }

    public static SessionId UnsafeFromGuid(Guid value) => new(value);

    public static Outcome<SessionId> FromString(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return Outcome.Failure<SessionId>(Errors.StringRequired);

        if (!Guid.TryParse(value, out var guid))
            return Outcome.Failure<SessionId>(Errors.InvalidFormat);

        return FromGuid(guid);
    }

    public override string ToString() => Value.ToString();

    public bool IsEmpty => Value == Guid.Empty;

    public static implicit operator Guid(SessionId sessionId) => sessionId.Value;

    public static explicit operator SessionId(Guid value) => UnsafeFromGuid(value);

    private static class Errors
    {
        public static readonly Fault Invalid = Fault.Validation
        (
            "SessionId.Invalid",
            "SessionId cannot be an empty GUID.");

        public static readonly Fault InvalidFormat = Fault.Validation
        (
            "SessionId.InvalidFormat",
            "The provided string is not a valid GUID format.");

        public static readonly Fault StringRequired = Fault.Validation
        (
            "SessionId.StringRequired",
            "SessionId requires a non-empty string."
        );
    }
}