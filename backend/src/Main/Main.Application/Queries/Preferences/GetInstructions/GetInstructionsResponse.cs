namespace Main.Application.Queries.Preferences.GetInstructions;

public sealed record GetInstructionsResponse(IReadOnlyList<InstructionReadModel> Instructions);