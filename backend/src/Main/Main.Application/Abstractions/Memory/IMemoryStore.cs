namespace Main.Application.Abstractions.Memory;

public interface IMemoryStore
{
    Task<string> SaveAsync(Guid userId, string content, MemoryCategory memoryCategory,
        CancellationToken cancellationToken);

    Task<IReadOnlyList<MemoryEntry>> GetRelevantAsync(Guid userId, string context, int limit,
        CancellationToken cancellationToken);

    Task<IReadOnlyList<MemoryEntry>> GetRecentAsync(Guid userId, int limit,
        CancellationToken cancellationToken);

    Task<int> GetCountAsync(Guid userId, CancellationToken cancellationToken);

    Task DeleteAsync(Guid userId, CancellationToken cancellationToken);

    Task DeleteAllAsync(Guid userId, CancellationToken cancellationToken);
}