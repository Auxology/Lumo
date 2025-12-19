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
            return Faults.Invalid;

        return new RecoveryKeyChainId(value);
    }

    public static RecoveryKeyChainId UnsafeFromGuid(Guid value) => new(value);

    public static Outcome<RecoveryKeyChainId> FromString(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return Faults.StringRequired;

        if (!Guid.TryParse(value, out var guid))
            return Faults.InvalidFormat;

        return FromGuid(guid);
    }

    public override string ToString() => Value.ToString();

    public bool IsEmpty => Value == Guid.Empty;

    public static implicit operator Guid(RecoveryKeyChainId recoveryKeyChainId) => recoveryKeyChainId.Value;

    public static explicit operator RecoveryKeyChainId(Guid value) => UnsafeFromGuid(value);

    private static class Faults
    {
        public static readonly Fault Invalid = Fault.Validation
        (
            title: "RecoveryKeyChainId.Invalid",
            detail: "RecoveryKeyChainId cannot be an empty GUID."
        );

        public static readonly Fault InvalidFormat = Fault.Validation
        (
            title: "RecoveryKeyChainId.InvalidFormat",
            detail: "The provided string is not a valid GUID format."
        );

        public static readonly Fault StringRequired = Fault.Validation
        (
            title: "RecoveryKeyChainId.StringRequired",
            detail: "RecoveryKeyChainId requires a non-empty string."
        );
    }
}