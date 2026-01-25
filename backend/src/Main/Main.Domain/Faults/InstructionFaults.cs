using SharedKernel;

namespace Main.Domain.Faults;

public static class InstructionFaults
{
    public static readonly Fault ContentEmpty = Fault.Validation
    (
        title: "Instruction.ContentEmpty",
        detail: "Content is required and cannot be empty."
    );

    public static readonly Fault ContentTooLong = Fault.Validation
    (
        title: "Instruction.ContentTooLong",
        detail: "Content exceeds the maximum allowed length."
    );

    public static readonly Fault ContentTooShort = Fault.Validation
    (
        title: "Instruction.ContentTooShort",
        detail: "Content is too short."
    );

    public static readonly Fault PreferenceIdRequired = Fault.Validation
    (
        title: "Instruction.PreferenceIdRequired",
        detail: "PreferenceId is required."
    );

    public static readonly Fault InstructionIdRequired = Fault.Validation
    (
        title: "Instruction.InstructionIdRequired",
        detail: "InstructionId is required."
    );
}