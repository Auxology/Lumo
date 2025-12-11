using Auth.Application.Users.RequestLogin;
using Auth.Domain.Constants;
using FluentValidation.TestHelper;

namespace Auth.Application.Tests.Users.RequestLogin;

public sealed class RequestLoginCommandValidatorTests
{
    private readonly RequestLoginCommandValidator _validator;

    public RequestLoginCommandValidatorTests()
    {
        _validator = new RequestLoginCommandValidator();
    }

    [Fact]
    public void Validate_WithValidData_ShouldPass()
    {
        RequestLoginCommand command = new
        (
            EmailAddress: "john@example.com"
        );

        TestValidationResult<RequestLoginCommand>? result = _validator.TestValidate(command);

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Validate_WithEmptyEmail_ShouldFail(string? email)
    {
        RequestLoginCommand command = new
        (
            EmailAddress: email!
        );

        TestValidationResult<RequestLoginCommand>? result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.EmailAddress);
    }

    [Theory]
    [InlineData("invalid-email")]
    [InlineData("@example.com")]
    [InlineData("john@")]
    public void Validate_WithInvalidEmailFormat_ShouldFail(string email)
    {
        RequestLoginCommand command = new
        (
            EmailAddress: email!
        );

        TestValidationResult<RequestLoginCommand>? result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.EmailAddress);
    }

    [Fact]
    public void Validate_WithTooLongEmail_ShouldFail()
    {
        string email = new string('a', UserConstants.MaxEmailLength) + "@example.com";

        RequestLoginCommand command = new
        (
            EmailAddress: email
        );

        TestValidationResult<RequestLoginCommand>? result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.EmailAddress);
    }
}
