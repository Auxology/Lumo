using FluentAssertions;

using Main.Domain.ValueObjects;

using SharedKernel;

namespace Main.Domain.Tests.ValueObjects;

public sealed class ChatIdTests
{
    private const string ValidChatId = "cht_01JGX123456789012345678901";
    private const string Prefix = "cht_";
    private const int ExpectedLength = 30;

    [Fact]
    public void From_WithValidId_ShouldReturnSuccess()
    {
        Outcome<ChatId> outcome = ChatId.From(ValidChatId);

        outcome.IsSuccess.Should().BeTrue();
        outcome.Value.Value.Should().Be(ValidChatId);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void From_WithEmptyOrWhitespace_ShouldReturnFailure(string? value)
    {
        Outcome<ChatId> outcome = ChatId.From(value);

        outcome.IsFailure.Should().BeTrue();
        outcome.Fault.Title.Should().Be("ChatId.Required");
    }

    [Theory]
    [InlineData("invalid_id")]
    [InlineData("cht_short")]
    [InlineData("cht_01JGX123456789012345678")] // 31 chars - too long
    [InlineData("cht_01JGX1234567890123456")] // 29 chars - too short
    [InlineData("xxx_01JGX12345678901234567")] // wrong prefix
    public void From_WithInvalidFormat_ShouldReturnFailure(string value)
    {
        Outcome<ChatId> outcome = ChatId.From(value);

        outcome.IsFailure.Should().BeTrue();
        outcome.Fault.Title.Should().Be("ChatId.InvalidFormat");
    }

    [Fact]
    public void UnsafeFrom_ShouldCreateWithoutValidation()
    {
        string invalidId = "invalid";

        ChatId result = ChatId.UnsafeFrom(invalidId);

        result.Value.Should().Be(invalidId);
    }

    [Fact]
    public void ToString_ShouldReturnValue()
    {
        ChatId id = ChatId.UnsafeFrom(ValidChatId);

        string result = id.ToString();

        result.Should().Be(ValidChatId);
    }

    [Fact]
    public void IsEmpty_WhenEmpty_ShouldReturnTrue()
    {
        ChatId id = ChatId.UnsafeFrom(string.Empty);

        id.IsEmpty.Should().BeTrue();
    }

    [Fact]
    public void IsEmpty_WhenNotEmpty_ShouldReturnFalse()
    {
        ChatId id = ChatId.UnsafeFrom(ValidChatId);

        id.IsEmpty.Should().BeFalse();
    }

    [Fact]
    public void PrefixValue_ShouldReturnPrefix()
    {
        ChatId.PrefixValue.Should().Be(Prefix);
    }

    [Fact]
    public void Length_ShouldReturnExpectedLength()
    {
        ChatId.Length.Should().Be(ExpectedLength);
    }

    [Fact]
    public void Equality_WithSameValue_ShouldBeEqual()
    {
        ChatId id1 = ChatId.UnsafeFrom(ValidChatId);
        ChatId id2 = ChatId.UnsafeFrom(ValidChatId);

        id1.Should().Be(id2);
    }

    [Fact]
    public void Equality_WithDifferentValue_ShouldNotBeEqual()
    {
        ChatId id1 = ChatId.UnsafeFrom("cht_01JGX123456789012345678901");
        ChatId id2 = ChatId.UnsafeFrom("cht_01JGX123456789012345678902");

        id1.Should().NotBe(id2);
    }

    [Fact]
    public void ValidId_ShouldHaveCorrectLength()
    {
        ValidChatId.Length.Should().Be(ExpectedLength);
    }

    [Fact]
    public void ValidId_ShouldStartWithPrefix()
    {
        ValidChatId.Should().StartWith(Prefix);
    }
}