using FluentAssertions;

using Main.Domain.ValueObjects;

using SharedKernel;

namespace Main.Domain.Tests.ValueObjects;

public sealed class MessageIdTests
{
    private const string ValidMessageId = "msg_01JGX123456789012345678901";
    private const string Prefix = "msg_";
    private const int ExpectedLength = 30;

    [Fact]
    public void From_WithValidId_ShouldReturnSuccess()
    {
        Outcome<MessageId> outcome = MessageId.From(ValidMessageId);

        outcome.IsSuccess.Should().BeTrue();
        outcome.Value.Value.Should().Be(ValidMessageId);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void From_WithEmptyOrWhitespace_ShouldReturnFailure(string? value)
    {
        Outcome<MessageId> outcome = MessageId.From(value);

        outcome.IsFailure.Should().BeTrue();
        outcome.Fault.Title.Should().Be("MessageId.Required");
    }

    [Theory]
    [InlineData("invalid_id")]
    [InlineData("msg_short")]
    [InlineData("msg_01JGX123456789012345678")] // 29 chars - too short
    [InlineData("msg_01JGX1234567890123456789")] // 31 chars - too long
    [InlineData("xxx_01JGX12345678901234567890")] // wrong prefix
    public void From_WithInvalidFormat_ShouldReturnFailure(string value)
    {
        Outcome<MessageId> outcome = MessageId.From(value);

        outcome.IsFailure.Should().BeTrue();
        outcome.Fault.Title.Should().Be("MessageId.InvalidFormat");
    }

    [Fact]
    public void UnsafeFrom_ShouldCreateWithoutValidation()
    {
        string invalidId = "invalid";

        MessageId result = MessageId.UnsafeFrom(invalidId);

        result.Value.Should().Be(invalidId);
    }

    [Fact]
    public void ToString_ShouldReturnValue()
    {
        MessageId id = MessageId.UnsafeFrom(ValidMessageId);

        string result = id.ToString();

        result.Should().Be(ValidMessageId);
    }

    [Fact]
    public void IsEmpty_WhenEmpty_ShouldReturnTrue()
    {
        MessageId id = MessageId.UnsafeFrom(string.Empty);

        id.IsEmpty.Should().BeTrue();
    }

    [Fact]
    public void IsEmpty_WhenNotEmpty_ShouldReturnFalse()
    {
        MessageId id = MessageId.UnsafeFrom(ValidMessageId);

        id.IsEmpty.Should().BeFalse();
    }

    [Fact]
    public void PrefixValue_ShouldReturnPrefix()
    {
        MessageId.PrefixValue.Should().Be(Prefix);
    }

    [Fact]
    public void Length_ShouldReturnExpectedLength()
    {
        MessageId.Length.Should().Be(ExpectedLength);
    }

    [Fact]
    public void Equality_WithSameValue_ShouldBeEqual()
    {
        MessageId id1 = MessageId.UnsafeFrom(ValidMessageId);
        MessageId id2 = MessageId.UnsafeFrom(ValidMessageId);

        id1.Should().Be(id2);
    }

    [Fact]
    public void Equality_WithDifferentValue_ShouldNotBeEqual()
    {
        MessageId id1 = MessageId.UnsafeFrom("msg_01JGX123456789012345678901");
        MessageId id2 = MessageId.UnsafeFrom("msg_01JGX123456789012345678902");

        id1.Should().NotBe(id2);
    }

    [Fact]
    public void ValidId_ShouldHaveCorrectLength()
    {
        ValidMessageId.Length.Should().Be(ExpectedLength);
    }

    [Fact]
    public void ValidId_ShouldStartWithPrefix()
    {
        ValidMessageId.Should().StartWith(Prefix);
    }
}
