namespace Main.Application.Abstractions.Instructions;

public sealed record InstructionEntry
(
    string Content,
    int Priority
);