using SharedKernel;

namespace Auth.Domain.ValueObjects;

public readonly record struct EmailAddress
{
    private const int MaxLength = 254;

    public string Value { get; }

    private EmailAddress(string value)
    {
        Value = value;
    }

    public static Outcome<EmailAddress> Create(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            return Faults.StringRequired;

        email = Normalize(email);

        if (email.Length > MaxLength)
            return Faults.TooLong;

        if (!IsValidFormat(email))
            return Faults.Invalid;

        return new EmailAddress(email);
    }

    public static EmailAddress UnsafeFromString(string email) => new(email);

#pragma warning disable CA1308
    private static string Normalize(string email) => email.Trim().ToLowerInvariant();
#pragma warning restore CA1308

    private static bool IsValidFormat(string email)
    {
        try
        {
            var addr = new System.Net.Mail.MailAddress(email);
            return addr.Address == email;
        }
        catch (FormatException)
        {
            return false;
        }
    }

    public override string ToString() => Value;

    public bool IsEmpty() => string.IsNullOrEmpty(Value);

    public static implicit operator string(EmailAddress email) => email.Value;

    private static class Faults
    {
        public static readonly Fault StringRequired = Fault.Validation
        (
            title: "EmailAddress.StringRequired",
            detail: "Email address requires a non-empty string."
        );

        public static readonly Fault Invalid = Fault.Validation
        (
            title: "EmailAddress.Invalid",
            detail: "Email address is not in a valid format."
        );

        public static readonly Fault TooLong = Fault.Validation
        (
            title: "EmailAddress.TooLong",
            detail: $"Email address cannot exceed {MaxLength} characters."
        );
    }
}