namespace Main.Api.Endpoints.Preferences.UpdateInstruction;

internal sealed record Request
(
    string InstructionId,
    string NewContent
);