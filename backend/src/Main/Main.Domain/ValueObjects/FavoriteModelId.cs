using SharedKernel;

namespace Main.Domain.ValueObjects;

public readonly record struct FavoriteModelId
{
    private const string Prefix = "fmd_";
    private const int TotalLength = 30;

    public string Value { get; }

    private FavoriteModelId(string value)
    {
        Value = value;
    }

    public static Outcome<FavoriteModelId> From(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return Faults.Required;

        if (!IsValid(value!))
            return Faults.InvalidFormat;

        return new FavoriteModelId(value);
    }

    public static FavoriteModelId UnsafeFrom(string value) => new(value);

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
            title: "FavoriteModelId.Required",
            detail: "FavoriteModelId is required."
        );

        public static readonly Fault InvalidFormat = Fault.Validation
        (
            title: "FavoriteModelId.InvalidFormat",
            detail: $"FavoriteModelId must start with '{Prefix}' and be {TotalLength} characters."
        );
    }
}