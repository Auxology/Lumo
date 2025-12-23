using Auth.Domain.ValueObjects;
using FluentAssertions;
using SharedKernel;

namespace Auth.Domain.Tests.ValueObjects;

public sealed class FingerprintTests
{
    private const string ValidIpAddress = "192.168.1.1";
    private const string ValidUserAgent = "Mozilla/5.0";
    private const string ValidTimezone = "Europe/London";
    private const string ValidLanguage = "en-US";
    private const string ValidNormalizedBrowser = "Chrome 120";
    private const string ValidNormalizedOs = "Windows 11";

    [Fact]
    public void Create_WithValidData_ShouldReturnSuccess()
    {
        Outcome<Fingerprint> outcome = Fingerprint.Create
        (
            ipAddress: ValidIpAddress,
            userAgent: ValidUserAgent,
            timezone: ValidTimezone,
            language: ValidLanguage,
            normalizedBrowser: ValidNormalizedBrowser,
            normalizedOs: ValidNormalizedOs
        );

        outcome.IsSuccess.Should().BeTrue();
        outcome.Value.IpAddress.Should().Be(ValidIpAddress);
        outcome.Value.UserAgent.Should().Be(ValidUserAgent);
        outcome.Value.Timezone.Should().Be(ValidTimezone);
        outcome.Value.Language.Should().Be(ValidLanguage);
        outcome.Value.NormalizedBrowser.Should().Be(ValidNormalizedBrowser);
        outcome.Value.NormalizedOs.Should().Be(ValidNormalizedOs);
    }

    [Fact]
    public void Create_WithValidData_ShouldComputeHash()
    {
        Outcome<Fingerprint> outcome = Fingerprint.Create
        (
            ipAddress: ValidIpAddress,
            userAgent: ValidUserAgent,
            timezone: ValidTimezone,
            language: ValidLanguage,
            normalizedBrowser: ValidNormalizedBrowser,
            normalizedOs: ValidNormalizedOs
        );

        outcome.IsSuccess.Should().BeTrue();
        outcome.Value.ComputedHash.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void Create_WithSameData_ShouldProduceSameHash()
    {
        Outcome<Fingerprint> outcome1 = Fingerprint.Create
        (
            ipAddress: ValidIpAddress,
            userAgent: ValidUserAgent,
            timezone: ValidTimezone,
            language: ValidLanguage,
            normalizedBrowser: ValidNormalizedBrowser,
            normalizedOs: ValidNormalizedOs
        );

        Outcome<Fingerprint> outcome2 = Fingerprint.Create
        (
            ipAddress: ValidIpAddress,
            userAgent: ValidUserAgent,
            timezone: ValidTimezone,
            language: ValidLanguage,
            normalizedBrowser: ValidNormalizedBrowser,
            normalizedOs: ValidNormalizedOs
        );

        outcome1.Value.ComputedHash.Should().Be(outcome2.Value.ComputedHash);
    }

    [Fact]
    public void Create_WithDifferentData_ShouldProduceDifferentHash()
    {
        Outcome<Fingerprint> outcome1 = Fingerprint.Create
        (
            ipAddress: ValidIpAddress,
            userAgent: ValidUserAgent,
            timezone: ValidTimezone,
            language: ValidLanguage,
            normalizedBrowser: ValidNormalizedBrowser,
            normalizedOs: ValidNormalizedOs
        );

        Outcome<Fingerprint> outcome2 = Fingerprint.Create
        (
            ipAddress: "10.0.0.1",
            userAgent: ValidUserAgent,
            timezone: ValidTimezone,
            language: ValidLanguage,
            normalizedBrowser: ValidNormalizedBrowser,
            normalizedOs: ValidNormalizedOs
        );

        outcome1.Value.ComputedHash.Should().NotBe(outcome2.Value.ComputedHash);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void Create_WithEmptyIpAddress_ShouldReturnFailure(string? ipAddress)
    {
        Outcome<Fingerprint> outcome = Fingerprint.Create
        (
            ipAddress: ipAddress!,
            userAgent: ValidUserAgent,
            timezone: ValidTimezone,
            language: ValidLanguage,
            normalizedBrowser: ValidNormalizedBrowser,
            normalizedOs: ValidNormalizedOs
        );

        outcome.IsFailure.Should().BeTrue();
        outcome.Fault.Title.Should().Be("Fingerprint.IpAddressRequired");
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void Create_WithEmptyUserAgent_ShouldReturnFailure(string? userAgent)
    {
        Outcome<Fingerprint> outcome = Fingerprint.Create
        (
            ipAddress: ValidIpAddress,
            userAgent: userAgent!,
            timezone: ValidTimezone,
            language: ValidLanguage,
            normalizedBrowser: ValidNormalizedBrowser,
            normalizedOs: ValidNormalizedOs
        );

        outcome.IsFailure.Should().BeTrue();
        outcome.Fault.Title.Should().Be("Fingerprint.UserAgentRequired");
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void Create_WithEmptyTimezone_ShouldReturnFailure(string? timezone)
    {
        Outcome<Fingerprint> outcome = Fingerprint.Create
        (
            ipAddress: ValidIpAddress,
            userAgent: ValidUserAgent,
            timezone: timezone!,
            language: ValidLanguage,
            normalizedBrowser: ValidNormalizedBrowser,
            normalizedOs: ValidNormalizedOs
        );

        outcome.IsFailure.Should().BeTrue();
        outcome.Fault.Title.Should().Be("Fingerprint.TimezoneRequired");
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void Create_WithEmptyLanguage_ShouldReturnFailure(string? language)
    {
        Outcome<Fingerprint> outcome = Fingerprint.Create
        (
            ipAddress: ValidIpAddress,
            userAgent: ValidUserAgent,
            timezone: ValidTimezone,
            language: language!,
            normalizedBrowser: ValidNormalizedBrowser,
            normalizedOs: ValidNormalizedOs
        );

        outcome.IsFailure.Should().BeTrue();
        outcome.Fault.Title.Should().Be("Fingerprint.LanguageRequired");
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void Create_WithEmptyNormalizedBrowser_ShouldReturnFailure(string? normalizedBrowser)
    {
        Outcome<Fingerprint> outcome = Fingerprint.Create
        (
            ipAddress: ValidIpAddress,
            userAgent: ValidUserAgent,
            timezone: ValidTimezone,
            language: ValidLanguage,
            normalizedBrowser: normalizedBrowser!,
            normalizedOs: ValidNormalizedOs
        );

        outcome.IsFailure.Should().BeTrue();
        outcome.Fault.Title.Should().Be("Fingerprint.NormalizedBrowserRequired");
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void Create_WithEmptyNormalizedOs_ShouldReturnFailure(string? normalizedOs)
    {
        Outcome<Fingerprint> outcome = Fingerprint.Create
        (
            ipAddress: ValidIpAddress,
            userAgent: ValidUserAgent,
            timezone: ValidTimezone,
            language: ValidLanguage,
            normalizedBrowser: ValidNormalizedBrowser,
            normalizedOs: normalizedOs!
        );

        outcome.IsFailure.Should().BeTrue();
        outcome.Fault.Title.Should().Be("Fingerprint.NormalizedOsRequired");
    }

    [Fact]
    public void Equality_WithSameValues_ShouldBeEqual()
    {
        Outcome<Fingerprint> outcome1 = Fingerprint.Create
        (
            ipAddress: ValidIpAddress,
            userAgent: ValidUserAgent,
            timezone: ValidTimezone,
            language: ValidLanguage,
            normalizedBrowser: ValidNormalizedBrowser,
            normalizedOs: ValidNormalizedOs
        );

        Outcome<Fingerprint> outcome2 = Fingerprint.Create
        (
            ipAddress: ValidIpAddress,
            userAgent: ValidUserAgent,
            timezone: ValidTimezone,
            language: ValidLanguage,
            normalizedBrowser: ValidNormalizedBrowser,
            normalizedOs: ValidNormalizedOs
        );

        outcome1.Value.Should().Be(outcome2.Value);
    }
}

