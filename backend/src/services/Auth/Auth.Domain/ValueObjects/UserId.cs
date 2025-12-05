using SharedKernel.ResultPattern;

namespace Auth.Domain.ValueObjects;

public readonly record struct UserId
{
    public Guid Value { get; }
    
    private UserId(Guid value)
    {
        Value = value;
    }
    
    public static UserId New() => new(Guid.NewGuid());

    public static Result<UserId> FromGuid(Guid value)
    {
        if (value == Guid.Empty)
            return UserIdErrors.Empty;
        
        return new UserId(value);
    }
    
    public static UserId UnsafeFromGuid(Guid value) => new(value);

    public static Result<UserId> FromString(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return UserIdErrors.NullOrEmpty;
        
        if (!Guid.TryParse(value, out var guid))
            return UserIdErrors.InvalidFormat;

        return FromGuid(guid);
    }
    
    public override string ToString() => Value.ToString();
    
    public bool IsEmpty() => Value == Guid.Empty;
}

internal static class UserIdErrors
{
    public static readonly Error Empty = Error.Validation
    (
        title: "UserId.Empty",
        detail: "UserId cannot be an empty GUID."
    );

    public static readonly Error NullOrEmpty = Error.Validation
    (
        title: "UserId.NullOrEmpty",
        detail: "UserId string cannot be null or empty."
    );

    public static readonly Error InvalidFormat = Error.Validation
    (
        title: "UserId.InvalidFormat",
        detail: "The provided string is not a valid GUID format."
    );
}
