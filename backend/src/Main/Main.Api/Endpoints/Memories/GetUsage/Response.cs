namespace Main.Api.Endpoints.Memories.GetUsage;

internal sealed record Response
(
    int CurrentCount,
    int MaxCount,
    bool IsAtLimit
);