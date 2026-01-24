namespace Main.Application.Queries.Memories.GetUsage;

public sealed record GetMemoryUsageResponse
(
    int CurrentCount,
    int MaxCount,
    bool IsAtLimit
);