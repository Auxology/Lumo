namespace Main.Application.Commands.Preferences.UpdateInstruction;

public sealed record UpdateInstructionResponse
(
    string InstructionId,
    string Content,
    int Priority,
    DateTimeOffset UpdatedAt
);