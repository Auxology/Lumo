using FluentAssertions;

using Main.Domain.ValueObjects;

using SharedKernel;

namespace Main.Domain.Tests.ValueObjects;

public sealed class StreamIdTests
{
    private const string ValidStreamId = "str_01JGX123456789012345678901";
    private const string Prefix = "str_";
    private const int ExpectedLength = 30;

    [Fact]
    public void From_WithValidId_ShouldReturnSuccess()
    {
        Outcome<StreamId> outcome = StreamId.From(ValidStreamId);

        outcome.IsSuccess.Should().BeTrue();
        outcome.Value.Value.Should().Be(ValidStreamId);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void From_WithEmptyOrWhitespace_ShouldReturnFailure(string? value)
    {
        Outcome<StreamId> outcome = StreamId.From(value);

        outcome.IsFailure.Should().BeTrue();
        outcome.Fault.Title.Should().Be("StreamId.Required");
    }

    [Theory]
    [InlineData("invalid_id")]
    [InlineData("str_short")]
    [InlineData("str_01JGX123456789012345678")] // wrong length
    [InlineData("xxx_01JGX12345678901234567890")] // wrong prefix
    public void From_WithInvalidFormat_ShouldReturnFailure(string value)
    {
        Outcome<StreamId> outcome = StreamId.From(value);

        outcome.IsFailure.Should().BeTrue();
        outcome.Fault.Title.Should().Be("StreamId.InvalidFormat");
    }

    [Fact]
    public void UnsafeFrom_ShouldCreateWithoutValidation()
    {
        string invalidId = "invalid";

        StreamId result = StreamId.UnsafeFrom(invalidId);

        result.Value.Should().Be(invalidId);
    }

    [Fact]
    public void ToString_ShouldReturnValue()
    {
        StreamId id = StreamId.UnsafeFrom(ValidStreamId);

        string result = id.ToString();

        result.Should().Be(ValidStreamId);
    }

    [Fact]
    public void IsEmpty_WhenEmpty_ShouldReturnTrue()
    {
        StreamId id = StreamId.UnsafeFrom(string.Empty);

        id.IsEmpty.Should().BeTrue();
    }

    [Fact]
    public void IsEmpty_WhenNotEmpty_ShouldReturnFalse()
    {
        StreamId id = StreamId.UnsafeFrom(ValidStreamId);

        id.IsEmpty.Should().BeFalse();
    }

    [Fact]
    public void PrefixValue_ShouldReturnPrefix()
    {
        StreamId.PrefixValue.Should().Be(Prefix);
    }

    [Fact]
    public void Length_ShouldReturnExpectedLength()
    {
        StreamId.Length.Should().Be(ExpectedLength);
    }

    [Fact]
    public void Equality_WithSameValue_ShouldBeEqual()
    {
        StreamId id1 = StreamId.UnsafeFrom(ValidStreamId);
        StreamId id2 = StreamId.UnsafeFrom(ValidStreamId);

        id1.Should().Be(id2);
    }

    [Fact]
    public void Equality_WithDifferentValue_ShouldNotBeEqual()
    {
        StreamId id1 = StreamId.UnsafeFrom("str_01JGX123456789012345678901");
        StreamId id2 = StreamId.UnsafeFrom("str_01JGX123456789012345678902");

        id1.Should().NotBe(id2);
    }

    [Fact]
    public void ValidId_ShouldHaveCorrectLength()
    {
        ValidStreamId.Length.Should().Be(ExpectedLength);
    }

    [Fact]
    public void ValidId_ShouldStartWithPrefix()
    {
        ValidStreamId.Should().StartWith(Prefix);
    }
}