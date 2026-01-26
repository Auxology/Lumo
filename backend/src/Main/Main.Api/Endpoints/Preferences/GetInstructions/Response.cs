namespace Main.Api.Endpoints.Preferences.GetInstructions;

internal sealed record InstructionDto
(
    string Id,
    string Content,
    int Priority,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt
);

internal sealed record Response(IReadOnlyList<InstructionDto> Instructions);