using SharedKernel.Application.Messaging;

namespace Main.Application.Queries.Models;

public sealed record GetAvailableModelsQuery : IQuery<GetAvailableModelsResponse>;