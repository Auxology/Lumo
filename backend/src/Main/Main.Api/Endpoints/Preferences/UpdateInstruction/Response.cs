namespace Main.Api.Endpoints.Preferences.UpdateInstruction;

internal sealed record Response
(
    string InstructionId,
    string Content,
    int Priority,
    DateTimeOffset UpdatedAt
);