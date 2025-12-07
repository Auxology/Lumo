using Auth.Domain.ValueObjects;
using SharedKernel.ResultPattern;
using Shouldly;

namespace Auth.Domain.Tests.ValueObjects;

public sealed class UserIdTests
{
    [Fact]
    public void New_ShouldCreateUniqueId()
    {
        UserId id1 = UserId.New();
        UserId id2 = UserId.New();

        id1.Value.ShouldNotBe(Guid.Empty);
        id2.Value.ShouldNotBe(Guid.Empty);
        
        id1.ShouldNotBe(id2);
    }

    [Fact]
    public void FromGuid_WithValidGuid_ShouldSucceed()
    {
        Guid guid = Guid.NewGuid();

        Result<UserId> result = UserId.FromGuid(guid);
        
        Guid userIdValue = result.Value.Value;

        result.IsSuccess.ShouldBeTrue();
        
        userIdValue.ShouldBe(guid);
    }

    [Fact]
    public void FromGuid_WithEmptyGuid_ShouldFail()
    {
        Result<UserId> result = UserId.FromGuid(Guid.Empty);

        result.IsFailure.ShouldBeTrue();
        
        result.Error.ShouldBe(UserIdErrors.Empty);
    }

    [Fact]
    public void FromString_WithValidString_ShouldSucceed()
    {
        Guid guid = Guid.NewGuid();
        
        string guidString = guid.ToString();

        Result<UserId> result = UserId.FromString(guidString);

        result.IsSuccess.ShouldBeTrue();
        
        result.Value.Value.ShouldBe(guid);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void FromString_WithNullOrEmpty_ShouldFail(string? value)
    {
        Result<UserId> result = UserId.FromString(value!);

        result.IsFailure.ShouldBeTrue();
        
        result.Error.ShouldBe(UserIdErrors.NullOrEmpty);
    }

    [Fact]
    public void FromString_WithInvalidFormat_ShouldFail()
    {
        Result<UserId> result = UserId.FromString("not-a-guid");

        result.IsFailure.ShouldBeTrue();
        
        result.Error.ShouldBe(UserIdErrors.InvalidFormat);
    }

    [Fact]
    public void IsEmpty_WithEmptyValue_ShouldReturnTrue()
    {
        UserId userId = UserId.UnsafeFromGuid(Guid.Empty);
        
        userId.IsEmpty().ShouldBeTrue();
    }

    [Fact]
    public void IsEmpty_WithValidValue_ShouldReturnFalse()
    {
        UserId userId = UserId.New();

        userId.IsEmpty().ShouldBeFalse();
    }
}