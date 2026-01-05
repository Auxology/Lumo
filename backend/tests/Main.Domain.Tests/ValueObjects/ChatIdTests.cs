using FluentAssertions;

using Main.Domain.ValueObjects;

using SharedKernel;

namespace Main.Domain.Tests.ValueObjects;

public sealed class ChatIdTests
{
    [Fact]
    public void New_ShouldCreateUniqueId()
    {
        ChatId id1 = ChatId.New();
        ChatId id2 = ChatId.New();

        id1.Should().NotBe(id2);
        id1.Value.Should().NotBe(Guid.Empty);
        id2.Value.Should().NotBe(Guid.Empty);
    }

    [Fact]
    public void FromGuid_WithValidGuid_ShouldReturnSuccess()
    {
        Guid guid = Guid.NewGuid();

        Outcome<ChatId> outcome = ChatId.FromGuid(guid);

        outcome.IsSuccess.Should().BeTrue();
        outcome.Value.Value.Should().Be(guid);
    }

    [Fact]
    public void FromGuid_WithEmptyGuid_ShouldReturnFailure()
    {
        Outcome<ChatId> outcome = ChatId.FromGuid(Guid.Empty);

        outcome.IsFailure.Should().BeTrue();
        outcome.Fault.Title.Should().Be("ChatId.Invalid");
    }

    [Fact]
    public void FromString_WithValidGuidString_ShouldReturnSuccess()
    {
        Guid guid = Guid.NewGuid();
        string guidString = guid.ToString();

        Outcome<ChatId> outcome = ChatId.FromString(guidString);

        outcome.IsSuccess.Should().BeTrue();
        outcome.Value.Value.Should().Be(guid);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void FromString_WithEmptyOrWhitespace_ShouldReturnFailure(string? value)
    {
        Outcome<ChatId> outcome = ChatId.FromString(value!);

        outcome.IsFailure.Should().BeTrue();
        outcome.Fault.Title.Should().Be("ChatId.StringRequired");
    }

    [Theory]
    [InlineData("not-a-guid")]
    [InlineData("12345")]
    [InlineData("xyz")]
    public void FromString_WithInvalidFormat_ShouldReturnFailure(string value)
    {
        Outcome<ChatId> outcome = ChatId.FromString(value);

        outcome.IsFailure.Should().BeTrue();
        outcome.Fault.Title.Should().Be("ChatId.InvalidFormat");
    }

    [Fact]
    public void FromString_WithEmptyGuidString_ShouldReturnFailure()
    {
        Outcome<ChatId> outcome = ChatId.FromString(Guid.Empty.ToString());

        outcome.IsFailure.Should().BeTrue();
        outcome.Fault.Title.Should().Be("ChatId.Invalid");
    }

    [Fact]
    public void UnsafeFromGuid_ShouldCreateWithoutValidation()
    {
        Guid guid = Guid.Empty;

        ChatId result = ChatId.UnsafeFromGuid(guid);

        result.Value.Should().Be(guid);
    }

    [Fact]
    public void ToString_ShouldReturnGuidString()
    {
        Guid guid = Guid.NewGuid();
        ChatId id = ChatId.UnsafeFromGuid(guid);

        string result = id.ToString();

        result.Should().Be(guid.ToString());
    }

    [Fact]
    public void IsEmpty_WhenEmpty_ShouldReturnTrue()
    {
        ChatId id = ChatId.UnsafeFromGuid(Guid.Empty);

        id.IsEmpty.Should().BeTrue();
    }

    [Fact]
    public void IsEmpty_WhenNotEmpty_ShouldReturnFalse()
    {
        ChatId id = ChatId.New();

        id.IsEmpty.Should().BeFalse();
    }

    [Fact]
    public void ImplicitConversion_ToGuid_ShouldReturnValue()
    {
        Guid guid = Guid.NewGuid();
        ChatId id = ChatId.UnsafeFromGuid(guid);

        Guid result = id;

        result.Should().Be(guid);
    }

    [Fact]
    public void ExplicitConversion_FromGuid_ShouldCreateId()
    {
        Guid guid = Guid.NewGuid();

        ChatId result = (ChatId)guid;

        result.Value.Should().Be(guid);
    }

    [Fact]
    public void Equality_WithSameValue_ShouldBeEqual()
    {
        Guid guid = Guid.NewGuid();
        ChatId id1 = ChatId.UnsafeFromGuid(guid);
        ChatId id2 = ChatId.UnsafeFromGuid(guid);

        id1.Should().Be(id2);
    }

    [Fact]
    public void Equality_WithDifferentValue_ShouldNotBeEqual()
    {
        ChatId id1 = ChatId.New();
        ChatId id2 = ChatId.New();

        id1.Should().NotBe(id2);
    }
}