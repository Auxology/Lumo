namespace Main.Api.Endpoints.Models.GetModels;

internal sealed record ModelDto
(
    string Id,
    string DisplayName,
    string Provider,
    bool IsDefault,
    int MaxContextTokens,
    bool SupportsVision
);

internal sealed record Response(IReadOnlyList<ModelDto> Models);