namespace Main.Application.Commands.Preferences.AddInstruction;

public sealed record AddInstructionResponse
(
    string PreferenceId,
    string InstructionId,
    string Content,
    int Priority,
    DateTimeOffset CreatedAt
);