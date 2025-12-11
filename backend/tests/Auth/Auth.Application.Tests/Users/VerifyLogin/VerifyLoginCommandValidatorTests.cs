using Auth.Application.Users.VerifyLogin;
using Auth.Domain.Constants;
using FluentValidation.TestHelper;

namespace Auth.Application.Tests.Users.VerifyLogin;

public sealed class VerifyLoginCommandValidatorTests
{
    private readonly VerifyLoginCommandValidator _validator;

    public VerifyLoginCommandValidatorTests()
    {
        _validator = new VerifyLoginCommandValidator();
    }

    [Fact]
    public void Validate_WithValidOtpToken_ShouldPass()
    {
        string validOtpToken = new('1', UserTokenConstants.OtpTokenLength);

        VerifyLoginCommand command = new
        (
            EmailAddress: "john@example.com",
            OtpToken: validOtpToken,
            MagicLinkToken: null
        );

        TestValidationResult<VerifyLoginCommand>? result = _validator.TestValidate(command);

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_WithValidMagicLinkToken_ShouldPass()
    {
        string validMagicLinkToken = new('a', UserTokenConstants.MagicLinkTokenLength);

        VerifyLoginCommand command = new
        (
            EmailAddress: "john@example.com",
            OtpToken: null,
            MagicLinkToken: validMagicLinkToken
        );

        TestValidationResult<VerifyLoginCommand>? result = _validator.TestValidate(command);

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Validate_WithEmptyEmail_ShouldFail(string? email)
    {
        string validOtpToken = new('1', UserTokenConstants.OtpTokenLength);

        VerifyLoginCommand command = new
        (
            EmailAddress: email!,
            OtpToken: validOtpToken,
            MagicLinkToken: null
        );

        TestValidationResult<VerifyLoginCommand>? result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.EmailAddress);
    }

    [Theory]
    [InlineData("invalid-email")]
    [InlineData("@example.com")]
    [InlineData("john@")]
    public void Validate_WithInvalidEmailFormat_ShouldFail(string email)
    {
        string validOtpToken = new('1', UserTokenConstants.OtpTokenLength);

        VerifyLoginCommand command = new
        (
            EmailAddress: email!,
            OtpToken: validOtpToken,
            MagicLinkToken: null
        );

        TestValidationResult<VerifyLoginCommand>? result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.EmailAddress);
    }

    [Fact]
    public void Validate_WithTooLongEmail_ShouldFail()
    {
        string email = new string('a', UserConstants.MaxEmailLength) + "@example.com";

        string validOtpToken = new string('1', UserTokenConstants.OtpTokenLength);

        VerifyLoginCommand command = new
        (
            EmailAddress: email,
            OtpToken: validOtpToken,
            MagicLinkToken: null
        );

        TestValidationResult<VerifyLoginCommand>? result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.EmailAddress);
    }

    [Fact]
    public void Validate_WithNeitherToken_ShouldFail()
    {
        VerifyLoginCommand command = new
        (
            EmailAddress: "john@example.com",
            OtpToken: null,
            MagicLinkToken: null
        );

        TestValidationResult<VerifyLoginCommand>? result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x);
    }

    [Fact]
    public void Validate_WithInvalidOtpTokenLength_ShouldFail()
    {
        VerifyLoginCommand command = new
        (
            EmailAddress: "john@example.com",
            OtpToken: "123",
            MagicLinkToken: null
        );

        TestValidationResult<VerifyLoginCommand>? result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.OtpToken);
    }

    [Fact]
    public void Validate_WithInvalidMagicLinkTokenLength_ShouldFail()
    {
        VerifyLoginCommand command = new
        (
            EmailAddress: "john@example.com",
            OtpToken: null,
            MagicLinkToken: "short"
        );

        TestValidationResult<VerifyLoginCommand>? result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.MagicLinkToken);
    }

    [Fact]
    public void Validate_WithBothTokens_ShouldPass()
    {
        string validOtpToken = new('1', UserTokenConstants.OtpTokenLength);

        string validMagicLinkToken = new('a', UserTokenConstants.MagicLinkTokenLength);

        VerifyLoginCommand command = new
        (
            EmailAddress: "john@example.com",
            OtpToken: validOtpToken,
            MagicLinkToken: validMagicLinkToken
        );

        TestValidationResult<VerifyLoginCommand>? result = _validator.TestValidate(command);

        result.ShouldNotHaveAnyValidationErrors();
    }
}
