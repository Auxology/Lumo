using Auth.Application.Users.SignUp;
using Auth.Domain.Constants;
using FluentValidation.TestHelper;

namespace Auth.Application.Tests.Users.SignUp;

public sealed class SignUpCommandValidatorTests
{
    private readonly SignUpCommandValidator _validator;

    public SignUpCommandValidatorTests()
    {
        _validator = new SignUpCommandValidator();
    }

    [Fact]
    public void Validate_WithValidData_ShouldPass()
    {
        SignUpCommand command = new
        (
            DisplayName: "John Doe",
            EmailAddress: "john@example.com"
        );

        TestValidationResult<SignUpCommand>? result = _validator.TestValidate(command);

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Validate_WithEmptyDisplayName_ShouldFail(string? displayName)
    {
        SignUpCommand command = new
        (
            DisplayName: displayName!,
            EmailAddress: "john@example.com"
        );

        TestValidationResult<SignUpCommand>? result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.DisplayName);
    }

    [Fact]
    public void Validate_WithTooLongDisplayName_ShouldFail()
    {
        string displayName = new('a', UserConstants.MaxDisplayNameLength + 1);

        SignUpCommand command = new
        (
            DisplayName: displayName,
            EmailAddress: "john@example.com"
        );

        TestValidationResult<SignUpCommand>? result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.DisplayName);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Validate_WithEmptyEmail_ShouldFail(string? email)
    {
        SignUpCommand command = new
        (
            DisplayName: "John Doe",
            EmailAddress: email!
        );

        TestValidationResult<SignUpCommand>? result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.EmailAddress);
    }

    [Theory]
    [InlineData("invalid-email")]
    [InlineData("@example.com")]
    [InlineData("john@")]
    public void Validate_WithInvalidEmailFormat_ShouldFail(string email)
    {
        SignUpCommand command = new
        (
            DisplayName: "John Doe",
            EmailAddress: email!
        );

        TestValidationResult<SignUpCommand>? result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.EmailAddress);
    }
}
