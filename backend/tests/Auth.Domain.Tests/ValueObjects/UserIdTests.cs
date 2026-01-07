using Auth.Domain.ValueObjects;

using FluentAssertions;

using SharedKernel;

namespace Auth.Domain.Tests.ValueObjects;

public sealed class UserIdTests
{
    [Fact]
    public void New_ShouldCreateUniqueId()
    {
        UserId id1 = UserId.New();
        UserId id2 = UserId.New();

        id1.Should().NotBe(id2);
        id1.Value.Should().NotBe(Guid.Empty);
        id2.Value.Should().NotBe(Guid.Empty);
    }

    [Fact]
    public void FromGuid_WithValidGuid_ShouldReturnSuccess()
    {
        Guid guid = Guid.NewGuid();

        Outcome<UserId> outcome = UserId.FromGuid(guid);

        outcome.IsSuccess.Should().BeTrue();
        outcome.Value.Value.Should().Be(guid);
    }

    [Fact]
    public void FromGuid_WithEmptyGuid_ShouldReturnFailure()
    {
        Outcome<UserId> outcome = UserId.FromGuid(Guid.Empty);

        outcome.IsFailure.Should().BeTrue();
        outcome.Fault.Title.Should().Be("UserId.Invalid");
    }

    [Fact]
    public void FromString_WithValidGuidString_ShouldReturnSuccess()
    {
        Guid guid = Guid.NewGuid();
        string guidString = guid.ToString();

        Outcome<UserId> outcome = UserId.FromString(guidString);

        outcome.IsSuccess.Should().BeTrue();
        outcome.Value.Value.Should().Be(guid);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void FromString_WithEmptyOrWhitespace_ShouldReturnFailure(string? value)
    {
        Outcome<UserId> outcome = UserId.FromString(value!);

        outcome.IsFailure.Should().BeTrue();
        outcome.Fault.Title.Should().Be("UserId.StringRequired");
    }

    [Theory]
    [InlineData("not-a-guid")]
    [InlineData("12345")]
    [InlineData("xyz")]
    public void FromString_WithInvalidFormat_ShouldReturnFailure(string value)
    {
        Outcome<UserId> outcome = UserId.FromString(value);

        outcome.IsFailure.Should().BeTrue();
        outcome.Fault.Title.Should().Be("UserId.InvalidFormat");
    }

    [Fact]
    public void FromString_WithEmptyGuidString_ShouldReturnFailure()
    {
        Outcome<UserId> outcome = UserId.FromString(Guid.Empty.ToString());

        outcome.IsFailure.Should().BeTrue();
        outcome.Fault.Title.Should().Be("UserId.Invalid");
    }

    [Fact]
    public void UnsafeFromGuid_ShouldCreateWithoutValidation()
    {
        Guid guid = Guid.Empty;

        UserId result = UserId.UnsafeFromGuid(guid);

        result.Value.Should().Be(guid);
    }

    [Fact]
    public void ToString_ShouldReturnGuidString()
    {
        Guid guid = Guid.NewGuid();
        UserId id = UserId.UnsafeFromGuid(guid);

        string result = id.ToString();

        result.Should().Be(guid.ToString());
    }

    [Fact]
    public void IsEmpty_WhenEmpty_ShouldReturnTrue()
    {
        UserId id = UserId.UnsafeFromGuid(Guid.Empty);

        id.IsEmpty.Should().BeTrue();
    }

    [Fact]
    public void IsEmpty_WhenNotEmpty_ShouldReturnFalse()
    {
        UserId id = UserId.New();

        id.IsEmpty.Should().BeFalse();
    }

    [Fact]
    public void ImplicitConversion_ToGuid_ShouldReturnValue()
    {
        Guid guid = Guid.NewGuid();
        UserId id = UserId.UnsafeFromGuid(guid);

        Guid result = id;

        result.Should().Be(guid);
    }

    [Fact]
    public void ExplicitConversion_FromGuid_ShouldCreateId()
    {
        Guid guid = Guid.NewGuid();

        UserId result = (UserId)guid;

        result.Value.Should().Be(guid);
    }

    [Fact]
    public void Equality_WithSameValue_ShouldBeEqual()
    {
        Guid guid = Guid.NewGuid();
        UserId id1 = UserId.UnsafeFromGuid(guid);
        UserId id2 = UserId.UnsafeFromGuid(guid);

        id1.Should().Be(id2);
    }

    [Fact]
    public void Equality_WithDifferentValue_ShouldNotBeEqual()
    {
        UserId id1 = UserId.New();
        UserId id2 = UserId.New();

        id1.Should().NotBe(id2);
    }
}