using FluentAssertions;

using Main.Domain.Aggregates;
using Main.Domain.Constants;
using Main.Domain.Entities;
using Main.Domain.Faults;
using Main.Domain.ValueObjects;

using SharedKernel;

namespace Main.Domain.Tests.Aggregates;

public sealed class PreferenceTests
{
    private static readonly DateTimeOffset UtcNow = DateTimeOffset.UtcNow;
    private static readonly Guid ValidUserId = Guid.NewGuid();
    private static readonly PreferenceId ValidPreferenceId = PreferenceId.UnsafeFrom("prf_01JGX123456789012345678901");
    private static readonly InstructionId ValidInstructionId = InstructionId.UnsafeFrom("ins_01JGX123456789012345678901");

    private static string ValidInstructionContent => new('a', InstructionConstants.MinContentLength);

    #region Create Tests

    [Fact]
    public void Create_WithValidData_ShouldReturnSuccess()
    {
        Outcome<Preference> outcome = Preference.Create(ValidPreferenceId, ValidUserId, UtcNow);

        outcome.IsSuccess.Should().BeTrue();
        outcome.Value.Id.Should().Be(ValidPreferenceId);
        outcome.Value.UserId.Should().Be(ValidUserId);
        outcome.Value.CreatedAt.Should().Be(UtcNow);
        outcome.Value.UpdatedAt.Should().Be(UtcNow);
        outcome.Value.Instructions.Should().BeEmpty();
    }

    [Fact]
    public void Create_WithEmptyUserId_ShouldReturnFailure()
    {
        Outcome<Preference> outcome = Preference.Create(ValidPreferenceId, Guid.Empty, UtcNow);

        outcome.IsFailure.Should().BeTrue();
        outcome.Fault.Should().Be(PreferenceFaults.UserIdRequired);
    }

    #endregion

    #region AddInstruction Tests

    [Fact]
    public void AddInstruction_WithValidContent_ShouldAddInstruction()
    {
        Preference preference = Preference.Create(ValidPreferenceId, ValidUserId, UtcNow).Value;
        DateTimeOffset updateTime = UtcNow.AddHours(1);

        Outcome<Instruction> outcome = preference.AddInstruction(ValidInstructionId, ValidInstructionContent, updateTime);

        outcome.IsSuccess.Should().BeTrue();
        preference.Instructions.Should().HaveCount(1);
        preference.Instructions.First().Content.Should().Be(ValidInstructionContent);
        preference.Instructions.First().Priority.Should().Be(PreferenceConstants.MinInstructionPriority);
        preference.UpdatedAt.Should().Be(updateTime);
    }

    [Fact]
    public void AddInstruction_MultipleInstructions_ShouldIncrementPriority()
    {
        Preference preference = Preference.Create(ValidPreferenceId, ValidUserId, UtcNow).Value;

        preference.AddInstruction(InstructionId.UnsafeFrom("ins_01JGX123456789012345678911"), ValidInstructionContent, UtcNow.AddMinutes(1));
        preference.AddInstruction(InstructionId.UnsafeFrom("ins_01JGX123456789012345678922"), ValidInstructionContent, UtcNow.AddMinutes(2));
        preference.AddInstruction(InstructionId.UnsafeFrom("ins_01JGX123456789012345678933"), ValidInstructionContent, UtcNow.AddMinutes(3));

        preference.Instructions.Should().HaveCount(3);

        List<Instruction> instructions = preference.Instructions.OrderBy(i => i.Priority).ToList();
        instructions[0].Priority.Should().Be(1);
        instructions[1].Priority.Should().Be(2);
        instructions[2].Priority.Should().Be(3);
    }

    [Fact]
    public void AddInstruction_WhenMaxReached_ShouldReturnFailure()
    {
        Preference preference = Preference.Create(ValidPreferenceId, ValidUserId, UtcNow).Value;

        for (int i = 0; i < PreferenceConstants.MaxInstructionCount; i++)
        {
            string instructionId = $"ins_01JGX12345678901234567{i:D4}";
            preference.AddInstruction(InstructionId.UnsafeFrom(instructionId), ValidInstructionContent, UtcNow);
        }

        Outcome<Instruction> outcome = preference.AddInstruction(
            InstructionId.UnsafeFrom("ins_01JGX123456789012345679999"),
            ValidInstructionContent,
            UtcNow);

        outcome.IsFailure.Should().BeTrue();
        outcome.Fault.Should().Be(PreferenceFaults.MaxInstructionsReached);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    public void AddInstruction_WithEmptyContent_ShouldReturnFailure(string content)
    {
        Preference preference = Preference.Create(ValidPreferenceId, ValidUserId, UtcNow).Value;

        Outcome<Instruction> outcome = preference.AddInstruction(ValidInstructionId, content, UtcNow);

        outcome.IsFailure.Should().BeTrue();
        outcome.Fault.Should().Be(InstructionFaults.ContentEmpty);
    }

    [Fact]
    public void AddInstruction_WithContentTooShort_ShouldReturnFailure()
    {
        Preference preference = Preference.Create(ValidPreferenceId, ValidUserId, UtcNow).Value;
        string shortContent = new('a', InstructionConstants.MinContentLength - 1);

        Outcome<Instruction> outcome = preference.AddInstruction(ValidInstructionId, shortContent, UtcNow);

        outcome.IsFailure.Should().BeTrue();
        outcome.Fault.Should().Be(InstructionFaults.ContentTooShort);
    }

    [Fact]
    public void AddInstruction_WithContentTooLong_ShouldReturnFailure()
    {
        Preference preference = Preference.Create(ValidPreferenceId, ValidUserId, UtcNow).Value;
        string longContent = new('a', InstructionConstants.MaxContentLength + 1);

        Outcome<Instruction> outcome = preference.AddInstruction(ValidInstructionId, longContent, UtcNow);

        outcome.IsFailure.Should().BeTrue();
        outcome.Fault.Should().Be(InstructionFaults.ContentTooLong);
    }

    #endregion

    #region UpdateInstruction Tests

    [Fact]
    public void UpdateInstruction_WithValidContent_ShouldUpdateInstruction()
    {
        Preference preference = Preference.Create(ValidPreferenceId, ValidUserId, UtcNow).Value;
        preference.AddInstruction(ValidInstructionId, ValidInstructionContent, UtcNow);
        string newContent = new('b', InstructionConstants.MinContentLength);
        DateTimeOffset updateTime = UtcNow.AddHours(1);

        Outcome outcome = preference.UpdateInstruction(ValidInstructionId, newContent, updateTime);

        outcome.IsSuccess.Should().BeTrue();
        preference.Instructions.First().Content.Should().Be(newContent);
        preference.UpdatedAt.Should().Be(updateTime);
    }

    [Fact]
    public void UpdateInstruction_WithNonExistentId_ShouldReturnFailure()
    {
        Preference preference = Preference.Create(ValidPreferenceId, ValidUserId, UtcNow).Value;
        InstructionId nonExistentId = InstructionId.UnsafeFrom("ins_01JGX999999999999999999999");

        Outcome outcome = preference.UpdateInstruction(nonExistentId, ValidInstructionContent, UtcNow);

        outcome.IsFailure.Should().BeTrue();
        outcome.Fault.Should().Be(PreferenceFaults.InstructionNotFound);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    public void UpdateInstruction_WithEmptyContent_ShouldReturnFailure(string content)
    {
        Preference preference = Preference.Create(ValidPreferenceId, ValidUserId, UtcNow).Value;
        preference.AddInstruction(ValidInstructionId, ValidInstructionContent, UtcNow);

        Outcome outcome = preference.UpdateInstruction(ValidInstructionId, content, UtcNow);

        outcome.IsFailure.Should().BeTrue();
        outcome.Fault.Should().Be(InstructionFaults.ContentEmpty);
    }

    [Fact]
    public void UpdateInstruction_WithContentTooShort_ShouldReturnFailure()
    {
        Preference preference = Preference.Create(ValidPreferenceId, ValidUserId, UtcNow).Value;
        preference.AddInstruction(ValidInstructionId, ValidInstructionContent, UtcNow);
        string shortContent = new('a', InstructionConstants.MinContentLength - 1);

        Outcome outcome = preference.UpdateInstruction(ValidInstructionId, shortContent, UtcNow);

        outcome.IsFailure.Should().BeTrue();
        outcome.Fault.Should().Be(InstructionFaults.ContentTooShort);
    }

    [Fact]
    public void UpdateInstruction_WithContentTooLong_ShouldReturnFailure()
    {
        Preference preference = Preference.Create(ValidPreferenceId, ValidUserId, UtcNow).Value;
        preference.AddInstruction(ValidInstructionId, ValidInstructionContent, UtcNow);
        string longContent = new('a', InstructionConstants.MaxContentLength + 1);

        Outcome outcome = preference.UpdateInstruction(ValidInstructionId, longContent, UtcNow);

        outcome.IsFailure.Should().BeTrue();
        outcome.Fault.Should().Be(InstructionFaults.ContentTooLong);
    }

    #endregion

    #region RemoveInstruction Tests

    [Fact]
    public void RemoveInstruction_WithExistingId_ShouldRemoveInstruction()
    {
        Preference preference = Preference.Create(ValidPreferenceId, ValidUserId, UtcNow).Value;
        preference.AddInstruction(ValidInstructionId, ValidInstructionContent, UtcNow);
        DateTimeOffset updateTime = UtcNow.AddHours(1);

        Outcome outcome = preference.RemoveInstruction(ValidInstructionId, updateTime);

        outcome.IsSuccess.Should().BeTrue();
        preference.Instructions.Should().BeEmpty();
        preference.UpdatedAt.Should().Be(updateTime);
    }

    [Fact]
    public void RemoveInstruction_WithNonExistentId_ShouldReturnFailure()
    {
        Preference preference = Preference.Create(ValidPreferenceId, ValidUserId, UtcNow).Value;
        InstructionId nonExistentId = InstructionId.UnsafeFrom("ins_01JGX999999999999999999999");

        Outcome outcome = preference.RemoveInstruction(nonExistentId, UtcNow);

        outcome.IsFailure.Should().BeTrue();
        outcome.Fault.Should().Be(PreferenceFaults.InstructionNotFound);
    }

    [Fact]
    public void RemoveInstruction_ShouldOnlyRemoveSpecifiedInstruction()
    {
        Preference preference = Preference.Create(ValidPreferenceId, ValidUserId, UtcNow).Value;
        InstructionId instructionId1 = InstructionId.UnsafeFrom("ins_01JGX123456789012345678911");
        InstructionId instructionId2 = InstructionId.UnsafeFrom("ins_01JGX123456789012345678922");

        preference.AddInstruction(instructionId1, ValidInstructionContent, UtcNow);
        preference.AddInstruction(instructionId2, ValidInstructionContent, UtcNow);

        preference.RemoveInstruction(instructionId1, UtcNow);

        preference.Instructions.Should().HaveCount(1);
        preference.Instructions.First().Id.Should().Be(instructionId2);
    }

    #endregion
}