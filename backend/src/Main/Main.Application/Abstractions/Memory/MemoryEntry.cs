namespace Main.Application.Abstractions.Memory;

public sealed record MemoryEntry
(
    string Id,
    string Content,
    MemoryCategory MemoryCategory,
    DateTimeOffset CreatedAt
);