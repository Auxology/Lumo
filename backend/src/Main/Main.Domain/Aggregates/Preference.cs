using System.Diagnostics.CodeAnalysis;

using Main.Domain.Constants;
using Main.Domain.Entities;
using Main.Domain.Faults;
using Main.Domain.ValueObjects;

using SharedKernel;

namespace Main.Domain.Aggregates;

public sealed class Preference : AggregateRoot<PreferenceId>
{
    private readonly List<Instruction> _instructions = [];

    public Guid UserId { get; private set; }

    public IReadOnlyCollection<Instruction> Instructions => [.. _instructions];

    public DateTimeOffset CreatedAt { get; private set; }

    public DateTimeOffset UpdatedAt { get; private set; }

    private Preference() { } // For EF Core

    [SetsRequiredMembers]
    public Preference
    (
        PreferenceId id,
        Guid userId,
        DateTimeOffset utcNow
    )
    {
        Id = id;
        UserId = userId;
        CreatedAt = utcNow;
        UpdatedAt = utcNow;
    }

    public static Outcome<Preference> Create
    (
        PreferenceId id,
        Guid userId,
        DateTimeOffset utcNow
    )
    {
        if (userId == Guid.Empty)
            return PreferenceFaults.UserIdRequired;

        Preference preference = new
        (
            id: id,
            userId: userId,
            utcNow: utcNow
        );

        return preference;
    }

    public Outcome<Instruction> AddInstruction
    (
        InstructionId instructionId,
        string content,
        DateTimeOffset utcNow
    )
    {
        if (_instructions.Count >= PreferenceConstants.MaxInstructionCount)
            return PreferenceFaults.MaxInstructionsReached;

        int priority = _instructions.Count == 0
            ? PreferenceConstants.MinInstructionPriority
            : _instructions.Max(i => i.Priority) + 1;

        Outcome<Instruction> instructionOutcome = Instruction.Create
        (
            id: instructionId,
            preferenceId: Id,
            content: content,
            priority: priority,
            utcNow: utcNow
        );

        if (instructionOutcome.IsFailure)
            return instructionOutcome.Fault;

        Instruction instruction = instructionOutcome.Value;

        _instructions.Add(instruction);
        UpdatedAt = utcNow;

        return instruction;
    }

    public Outcome UpdateInstruction
    (
        InstructionId instructionId,
        string newContent,
        DateTimeOffset utcNow
    )
    {
        Instruction? instruction = _instructions
            .FirstOrDefault(i => i.Id == instructionId);

        if (instruction is null)
            return PreferenceFaults.InstructionNotFound;

        Outcome updateOutcome = instruction.UpdateContent(newContent, utcNow);

        if (updateOutcome.IsFailure)
            return updateOutcome.Fault;

        UpdatedAt = utcNow;

        return Outcome.Success();
    }

    public Outcome RemoveInstruction
    (
        InstructionId instructionId,
        DateTimeOffset utcNow
    )
    {
        Instruction? instruction = _instructions
            .FirstOrDefault(i => i.Id == instructionId);

        if (instruction is null)
            return PreferenceFaults.InstructionNotFound;

        _instructions.Remove(instruction);
        UpdatedAt = utcNow;

        return Outcome.Success();
    }
}