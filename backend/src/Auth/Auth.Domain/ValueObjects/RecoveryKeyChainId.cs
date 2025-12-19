using SharedKernel;

namespace Auth.Domain.ValueObjects;

public readonly record struct RecoveryKeyChainId
{
    public Guid Value { get; }

    private RecoveryKeyChainId(Guid value)
    {
        Value = value;
    }

    public static RecoveryKeyChainId New() => new(Guid.NewGuid());

    public static Outcome<RecoveryKeyChainId> FromGuid(Guid value)
    {
        if (value == Guid.Empty)
            return Outcome.Failure<RecoveryKeyChainId>(Errors.Invalid);

        return Outcome.Success(new RecoveryKeyChainId(value));
    }

    public static RecoveryKeyChainId UnsafeFromGuid(Guid value) => new(value);

    public static Outcome<RecoveryKeyChainId> FromString(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return Outcome.Failure<RecoveryKeyChainId>(Errors.StringRequired);

        if (!Guid.TryParse(value, out var guid))
            return Outcome.Failure<RecoveryKeyChainId>(Errors.InvalidFormat);

        return FromGuid(guid);
    }

    public override string ToString() => Value.ToString();

    public bool IsEmpty => Value == Guid.Empty;

    public static implicit operator Guid(RecoveryKeyChainId recoveryKeyChainId) => recoveryKeyChainId.Value;

    public static explicit operator RecoveryKeyChainId(Guid value) => UnsafeFromGuid(value);

    private static class Errors
    {
        public static readonly Fault Invalid = Fault.Validation
        (
            "RecoveryKeyChainId.Invalid",
            "RecoveryKeyChainId cannot be an empty GUID.");

        public static readonly Fault InvalidFormat = Fault.Validation
        (
            "RecoveryKeyChainId.InvalidFormat",
            "The provided string is not a valid GUID format.");

        public static readonly Fault StringRequired = Fault.Validation
        (
            "RecoveryKeyChainId.StringRequired",
            "RecoveryKeyChainId requires a non-empty string."
        );
    }
}