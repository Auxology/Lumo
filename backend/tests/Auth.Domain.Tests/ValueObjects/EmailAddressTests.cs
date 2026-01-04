using Auth.Domain.ValueObjects;

using FluentAssertions;

using SharedKernel;

namespace Auth.Domain.Tests.ValueObjects;

public sealed class EmailAddressTests
{
    [Fact]
    public void Create_WithValidEmail_ShouldReturnSuccess()
    {
        string email = "test@example.com";

        Outcome<EmailAddress> outcome = EmailAddress.Create(email);

        outcome.IsSuccess.Should().BeTrue();
        outcome.Value.Value.Should().Be(email);
    }

    [Fact]
    public void Create_WithValidEmail_ShouldNormalizeToLowerCase()
    {
        string email = "TEST@EXAMPLE.COM";

        Outcome<EmailAddress> outcome = EmailAddress.Create(email);

        outcome.IsSuccess.Should().BeTrue();
        outcome.Value.Value.Should().Be("test@example.com");
    }

    [Fact]
    public void Create_WithValidEmail_ShouldTrimWhitespace()
    {
        string email = "  test@example.com  ";

        Outcome<EmailAddress> outcome = EmailAddress.Create(email);

        outcome.IsSuccess.Should().BeTrue();
        outcome.Value.Value.Should().Be("test@example.com");
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void Create_WithEmptyOrWhitespace_ShouldReturnFailure(string? email)
    {
        Outcome<EmailAddress> outcome = EmailAddress.Create(email!);

        outcome.IsFailure.Should().BeTrue();
        outcome.Fault.Title.Should().Be("EmailAddress.StringRequired");
    }

    [Theory]
    [InlineData("notanemail")]
    [InlineData("@example.com")]
    [InlineData("test@")]
    [InlineData("test@.com")]
    public void Create_WithInvalidFormat_ShouldReturnFailure(string email)
    {
        Outcome<EmailAddress> outcome = EmailAddress.Create(email);

        outcome.IsFailure.Should().BeTrue();
        outcome.Fault.Title.Should().Be("EmailAddress.Invalid");
    }

    [Fact]
    public void Create_WithTooLongEmail_ShouldReturnFailure()
    {
        string longLocalPart = new('a', 250);
        string email = $"{longLocalPart}@example.com";

        Outcome<EmailAddress> outcome = EmailAddress.Create(email);

        outcome.IsFailure.Should().BeTrue();
        outcome.Fault.Title.Should().Be("EmailAddress.TooLong");
    }

    [Fact]
    public void UnsafeFromString_ShouldCreateWithoutValidation()
    {
        string email = "any-value";

        EmailAddress result = EmailAddress.UnsafeFromString(email);

        result.Value.Should().Be(email);
    }

    [Fact]
    public void ToString_ShouldReturnValue()
    {
        EmailAddress email = EmailAddress.UnsafeFromString("test@example.com");

        string result = email.ToString();

        result.Should().Be("test@example.com");
    }

    [Fact]
    public void IsEmpty_WhenEmpty_ShouldReturnTrue()
    {
        EmailAddress email = EmailAddress.UnsafeFromString(string.Empty);

        email.IsEmpty().Should().BeTrue();
    }

    [Fact]
    public void IsEmpty_WhenNotEmpty_ShouldReturnFalse()
    {
        EmailAddress email = EmailAddress.UnsafeFromString("test@example.com");

        email.IsEmpty().Should().BeFalse();
    }

    [Fact]
    public void ImplicitConversion_ToString_ShouldReturnValue()
    {
        EmailAddress email = EmailAddress.UnsafeFromString("test@example.com");

        string result = email;

        result.Should().Be("test@example.com");
    }

    [Fact]
    public void Equality_WithSameValue_ShouldBeEqual()
    {
        EmailAddress email1 = EmailAddress.UnsafeFromString("test@example.com");
        EmailAddress email2 = EmailAddress.UnsafeFromString("test@example.com");

        email1.Should().Be(email2);
    }

    [Fact]
    public void Equality_WithDifferentValue_ShouldNotBeEqual()
    {
        EmailAddress email1 = EmailAddress.UnsafeFromString("test1@example.com");
        EmailAddress email2 = EmailAddress.UnsafeFromString("test2@example.com");

        email1.Should().NotBe(email2);
    }
}