namespace Main.Api.Endpoints.Preferences.AddInstruction;

internal sealed record Response
(
    string PreferenceId,
    string InstructionId,
    string Content,
    int Priority,
    DateTimeOffset CreatedAt
);