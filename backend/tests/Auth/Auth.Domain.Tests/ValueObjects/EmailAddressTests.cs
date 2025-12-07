using Auth.Domain.Constants;
using Auth.Domain.ValueObjects;
using SharedKernel.ResultPattern;
using Shouldly;

namespace Auth.Domain.Tests.ValueObjects;

public sealed class EmailAddressTests
{
    [Theory]
    [InlineData("test@example.com")]
    [InlineData("user.name@domain.co.uk")]
    [InlineData("USER@DOMAIN.COM")]
    public void Create_WithValidEmail_ShouldSucceed(string validEmail)
    {
        Result<EmailAddress> result = EmailAddress.Create(validEmail);

        string emailString = result.Value.Value;
        
        result.IsSuccess.ShouldBeTrue();

        emailString.ShouldBe(emailString.Trim().ToLowerInvariant());
    }
    
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WithNullOrEmpty_ShouldFail(string? email)
    {
        Result<EmailAddress> result = EmailAddress.Create(email!);

        result.IsFailure.ShouldBeTrue();
        
        result.Error.ShouldBe(EmailAddressErrors.NullOrEmpty);
    }
    
    [Fact]
    public void Create_WithTooLongEmail_ShouldFail()
    {
        string longEmail = new string('a', UserConstants.MaxEmailLength) + "@b.com";

        Result<EmailAddress> result = EmailAddress.Create(longEmail);

        result.IsFailure.ShouldBeTrue();
        
        result.Error.ShouldBe(EmailAddressErrors.TooLong);
    }
    
    [Fact]
    public void Create_ShouldNormalizeEmail()
    {
        const string email = "  TEST@EXAMPLE.COM  ";

        Result<EmailAddress> result = EmailAddress.Create(email);

        result.IsSuccess.ShouldBeTrue();
        
        result.Value.Value.ShouldBe("test@example.com");
    }
    
    [Fact]
    public void IsEmpty_WithDefaultValue_ShouldReturnTrue()
    {
        EmailAddress emailAddress = default;

        emailAddress.IsEmpty().ShouldBeTrue();
    }

    [Fact]
    public void UnsafeFromString_ShouldBypassValidation()
    {
        const string invalidEmail = "not-valid";

        EmailAddress emailAddress = EmailAddress.UnsafeFromString(invalidEmail);

        emailAddress.Value.ShouldBe(invalidEmail);
    }
}