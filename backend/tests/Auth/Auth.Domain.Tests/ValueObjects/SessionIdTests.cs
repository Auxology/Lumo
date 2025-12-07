using Auth.Domain.ValueObjects;
using SharedKernel.ResultPattern;
using Shouldly;

namespace Auth.Domain.Tests.ValueObjects;

public sealed class SessionIdTests
{
    [Fact]
    public void New_ShouldCreateUniqueId()
    {
        SessionId id1 = SessionId.New();
        SessionId id2 = SessionId.New();

        id1.Value.ShouldNotBe(Guid.Empty);
        id2.Value.ShouldNotBe(Guid.Empty);

        id1.ShouldNotBe(id2);
    }

    [Fact]
    public void FromGuid_WithValidGuid_ShouldSucceed()
    {
        Guid guid = Guid.NewGuid();

        Result<SessionId> result = SessionId.FromGuid(guid);

        Guid sessionIdValue = result.Value.Value;

        result.IsSuccess.ShouldBeTrue();

        sessionIdValue.ShouldBe(guid);
    }

    [Fact]
    public void FromGuid_WithEmptyGuid_ShouldFail()
    {
        Result<SessionId> result = SessionId.FromGuid(Guid.Empty);

        result.IsFailure.ShouldBeTrue();

        result.Error.ShouldBe(SessionIdErrors.Empty);
    }

    [Fact]
    public void FromString_WithValidString_ShouldSucceed()
    {
        Guid guid = Guid.NewGuid();

        string guidString = guid.ToString();

        Result<SessionId> result = SessionId.FromString(guidString);

        result.IsSuccess.ShouldBeTrue();

        result.Value.Value.ShouldBe(guid);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void FromString_WithNullOrEmpty_ShouldFail(string? value)
    {
        Result<SessionId> result = SessionId.FromString(value!);

        result.IsFailure.ShouldBeTrue();

        result.Error.ShouldBe(SessionIdErrors.NullOrEmpty);
    }

    [Fact]
    public void FromString_WithInvalidFormat_ShouldFail()
    {
        Result<SessionId> result = SessionId.FromString("not-a-guid");

        result.IsFailure.ShouldBeTrue();

        result.Error.ShouldBe(SessionIdErrors.InvalidFormat);
    }

    [Fact]
    public void IsEmpty_WithEmptyValue_ShouldReturnTrue()
    {
        SessionId sessionId = SessionId.UnsafeFromGuid(Guid.Empty);

        sessionId.IsEmpty().ShouldBeTrue();
    }

    [Fact]
    public void IsEmpty_WithValidValue_ShouldReturnFalse()
    {
        SessionId sessionId = SessionId.New();

        sessionId.IsEmpty().ShouldBeFalse();
    }
}
