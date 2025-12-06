using Auth.Domain.Constants;
using SharedKernel.ResultPattern;

namespace Auth.Domain.ValueObjects;

public readonly record struct EmailAddress
{
    public string Value { get; }

    private EmailAddress(string value)
    {
        Value = value;
    }

    public static Result<EmailAddress> Create(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            return EmailAddressErrors.NullOrEmpty;

        email = Normalize(email);

        Result validation = Validate(email);

        if (validation.IsFailure)
            return Result.Failure<EmailAddress>(validation.Error);

        return Result.Success(new EmailAddress(email));
    }

    private static Result Validate(string email)
    {
        if (email.Length > UserConstants.MaxEmailLength)
            return EmailAddressErrors.TooLong;

        try
        {
            _ = new System.Net.Mail.MailAddress(email);

            return Result.Success();
        }
        catch (FormatException)
        {
            return EmailAddressErrors.Invalid;
        }
    }

    public static EmailAddress UnsafeFromString(string email) => new(email);
    
    private static string Normalize(string email) => email.Trim().ToLowerInvariant();

    public override string ToString() => Value;
    
    public bool IsEmpty() => string.IsNullOrEmpty(Value);
}

internal static class EmailAddressErrors
{
    public static readonly Error NullOrEmpty = Error.Validation
    (
        title: "EmailAddress.NullOrEmpty",
        detail: "Email address requires a non-empty string to construct itself."
    );

    public static readonly Error Invalid = Error.Validation
    (
        title: "EmailAddress.Invalid",
        detail: "Email address is not in a valid format."
    );

    public static readonly Error TooLong = Error.Validation
    (
        title: "EmailAddress.TooLong",
        detail: $"Email address cannot be longer than {UserConstants.MaxEmailLength} characters."
    );
}