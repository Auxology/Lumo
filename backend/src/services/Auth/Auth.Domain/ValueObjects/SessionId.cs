using SharedKernel.ResultPattern;

namespace Auth.Domain.ValueObjects;

public readonly record struct SessionId
{
    public Guid Value { get; }
    
    private SessionId(Guid value)
    {
        Value = value;
    }
    
    public static SessionId New() => new(Guid.NewGuid());

    public static Result<SessionId> FromGuid(Guid value)
    {
        if (value == Guid.Empty)
            return SessionIdErrors.Empty;
        
        return new SessionId(value);
    }
    
    public static SessionId UnsafeFromGuid(Guid value) => new(value);

    public static Result<SessionId> FromString(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return SessionIdErrors.NullOrEmpty;
        
        if (!Guid.TryParse(value, out var guid))
            return SessionIdErrors.InvalidFormat;

        return FromGuid(guid);
    }
    
    public override string ToString() => Value.ToString();
    
    public bool IsEmpty() => Value == Guid.Empty;
}

internal static class SessionIdErrors
{
    public static readonly Error Empty = Error.Validation
    (
        title: "SessionId.Empty",
        detail: "SessionId cannot be an empty GUID."
    );

    public static readonly Error NullOrEmpty = Error.Validation
    (
        title: "SessionId.NullOrEmpty",
        detail: "SessionId string cannot be null or empty."
    );

    public static readonly Error InvalidFormat = Error.Validation
    (
        title: "SessionId.InvalidFormat",
        detail: "The provided string is not a valid GUID format."
    );
}