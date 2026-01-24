using SharedKernel.Application.Messaging;

namespace Main.Application.Queries.Memories.GetUsage;

public sealed record GetMemoryUsageQuery : IQuery<GetMemoryUsageResponse>;