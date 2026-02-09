using FluentAssertions;

namespace SharedKernel.Tests;

public sealed class OutcomeTests
{
    [Fact]
    public void Success_ShouldCreateSuccessfulOutcome()
    {
        Outcome outcome = Outcome.Success();

        outcome.IsSuccess.Should().BeTrue();
        outcome.IsFailure.Should().BeFalse();
        outcome.Fault.Should().Be(Fault.None);
    }

    [Fact]
    public void Failure_ShouldCreateFailedOutcome()
    {
        Fault fault = Fault.Problem("Test.Error", "Something went wrong");

        Outcome outcome = Outcome.Failure(fault);

        outcome.IsSuccess.Should().BeFalse();
        outcome.IsFailure.Should().BeTrue();
        outcome.Fault.Should().Be(fault);
    }

    [Fact]
    public void ImplicitConversion_FromFault_ShouldCreateFailedOutcome()
    {
        Fault fault = Fault.NotFound("Test.NotFound", "Resource not found");

        Outcome outcome = fault;

        outcome.IsFailure.Should().BeTrue();
        outcome.Fault.Should().Be(fault);
    }

    [Fact]
    public void ImplicitConversion_FromFaultNone_ShouldCreateSuccessfulOutcome()
    {
        Outcome outcome = Fault.None;

        outcome.IsSuccess.Should().BeTrue();
        outcome.Fault.Should().Be(Fault.None);
    }
}

public sealed class OutcomeOfTTests
{
    [Fact]
    public void Success_ShouldCreateSuccessfulOutcomeWithValue()
    {
        string value = "test value";

        Outcome<string> outcome = Outcome.Success(value);

        outcome.IsSuccess.Should().BeTrue();
        outcome.IsFailure.Should().BeFalse();
        outcome.Value.Should().Be(value);
        outcome.Fault.Should().Be(Fault.None);
    }

    [Fact]
    public void Failure_ShouldCreateFailedOutcome()
    {
        Fault fault = Fault.Validation("Test.Invalid", "Invalid input");

        Outcome<string> outcome = Outcome.Failure<string>(fault);

        outcome.IsSuccess.Should().BeFalse();
        outcome.IsFailure.Should().BeTrue();
        outcome.Fault.Should().Be(fault);
    }

    [Fact]
    public void Value_WhenFailure_ShouldThrowInvalidOperationException()
    {
        Fault fault = Fault.Problem("Test.Error", "Error occurred");
        Outcome<string> outcome = Outcome.Failure<string>(fault);

        Action act = () => _ = outcome.Value;

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("The value of a failed outcome cannot be accessed.");
    }

    [Fact]
    public void ImplicitConversion_FromValue_ShouldCreateSuccessfulOutcome()
    {
        string value = "implicit value";

        Outcome<string> outcome = value;

        outcome.IsSuccess.Should().BeTrue();
        outcome.Value.Should().Be(value);
    }

    [Fact]
    public void ImplicitConversion_FromNull_ShouldCreateFailedOutcomeWithNullValueFault()
    {
        string? value = null;

        Outcome<string> outcome = value;

        outcome.IsFailure.Should().BeTrue();
        outcome.Fault.Should().Be(Fault.NullValue);
    }

    [Fact]
    public void ImplicitConversion_FromFault_ShouldCreateFailedOutcome()
    {
        Fault fault = Fault.Conflict("Test.Conflict", "Resource conflict");

        Outcome<int> outcome = fault;

        outcome.IsFailure.Should().BeTrue();
        outcome.Fault.Should().Be(fault);
    }
}