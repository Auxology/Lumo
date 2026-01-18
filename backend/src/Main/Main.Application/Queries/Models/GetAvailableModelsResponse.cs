namespace Main.Application.Queries.Models;

public sealed record GetAvailableModelsResponse(IReadOnlyList<AvailableModelDto> Models); 