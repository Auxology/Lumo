using System.ClientModel;

using Main.Application.Abstractions.Memory;
using Main.Infrastructure.Data;
using Main.Infrastructure.Data.Entities;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

using OpenAI.Embeddings;

using Pgvector;
using Pgvector.EntityFrameworkCore;

using SharedKernel;

namespace Main.Infrastructure.Memory;

internal sealed class MemoryStore(
    MainDbContext dbContext,
    EmbeddingClient embeddingClient,
    IDateTimeProvider dateTimeProvider,
    ILogger<MemoryStore> logger) : IMemoryStore
{
    public async Task<string> SaveAsync(Guid userId, string content, MemoryCategory memoryCategory,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(content))
            throw new ArgumentException("Memory content cannot be empty.", nameof(content));

        if (content.Length > MemoryConstants.MaxContentLength)
            throw new ArgumentException(
                $"Memory content exceeds maximum length of {MemoryConstants.MaxContentLength}.",
                nameof(content));

        int count = await dbContext.Memories
            .CountAsync(m => m.UserId == userId, cancellationToken: cancellationToken);

#pragma warning disable S1135
        // TODO: Implement memory merging instead of just limiting the count
#pragma warning restore S1135
        if (count >= MemoryConstants.MaxMemoriesPerUser)
            throw new InvalidOperationException(
                $"User has reached the maximum of {MemoryConstants.MaxMemoriesPerUser} memories.");

        float[] embedding = await GenerateEmbeddingAsync(content, cancellationToken);

        MemoryRecord memoryRecord = new()
        {
            Id = $"mem_{Ulid.NewUlid()}",
            UserId = userId,
            Content = content,
            Category = memoryCategory,
            Embedding = new Vector(embedding),
            CreatedAt = dateTimeProvider.UtcNow,
        };

        dbContext.Memories.Add(memoryRecord);
        await dbContext.SaveChangesAsync(cancellationToken);

        logger.LogInformation
        (
            "Saved memory {MemoryId} for user {UserId} with category {Category}",
            memoryRecord.Id, userId, memoryRecord
        );

        return memoryRecord.Id;
    }

    public async Task<IReadOnlyList<MemoryEntry>> GetRelevantAsync(Guid userId, string context, int limit,
        CancellationToken cancellationToken)
    {
        float[]? queryEmbedding;

        try
        {
            queryEmbedding = await GenerateEmbeddingAsync(context, cancellationToken);
        }
        catch (ClientResultException exception)
        {
            logger.LogWarning(exception, "Failed to generate query embedding (API error), falling back to recent memories");
            return await GetRecentAsync(userId, limit, cancellationToken);
        }
        catch (HttpRequestException exception)
        {
            logger.LogWarning(exception, "Failed to generate query embedding (network error), falling back to recent memories");
            return await GetRecentAsync(userId, limit, cancellationToken);
        }

        Vector queryVector = new(queryEmbedding);

        List<MemoryRecord> memoryRecords = await dbContext.Memories
            .Where(m => m.UserId == userId)
            .OrderByDescending(m => m.Embedding.CosineDistance(queryVector))
            .Take(limit)
            .ToListAsync(cancellationToken);

        return [.. memoryRecords.Select(ToEntry)];
    }

    public async Task<IReadOnlyList<MemoryEntry>> GetRecentAsync(Guid userId, int limit,
        CancellationToken cancellationToken)
    {
        List<MemoryRecord> records = await dbContext.Memories
            .Where(m => m.UserId == userId)
            .OrderByDescending(m => m.CreatedAt)
            .Take(limit)
            .ToListAsync(cancellationToken);

        return [.. records.Select(ToEntry)];
    }

    public async Task<int> GetCountAsync(Guid userId, CancellationToken cancellationToken) =>
        await dbContext.Memories
            .CountAsync(m => m.UserId == userId, cancellationToken: cancellationToken);

    public async Task DeleteAsync(Guid userId, CancellationToken cancellationToken)
    {
        MemoryRecord? memoryRecord = await dbContext.Memories
            .FirstOrDefaultAsync(m => m.UserId == userId, cancellationToken: cancellationToken);

        if (memoryRecord is not null)
        {
            dbContext.Memories.Remove(memoryRecord);
            await dbContext.SaveChangesAsync(cancellationToken);

            logger.LogInformation("Deleted memory {MemoryId} for user {UserId}", memoryRecord.Id, userId);
        }
    }

    public async Task DeleteAllAsync(Guid userId, CancellationToken cancellationToken)
    {
        await dbContext.Memories
            .Where(m => m.UserId == userId)
            .ExecuteDeleteAsync(cancellationToken);

        logger.LogInformation("Deleted all memories for user {UserId}", userId);
    }

    private async Task<float[]> GenerateEmbeddingAsync(string content, CancellationToken cancellationToken)
    {
        try
        {
            OpenAIEmbedding embedding =
                await embeddingClient.GenerateEmbeddingAsync(content, cancellationToken: cancellationToken);

            return embedding.ToFloats().ToArray();
        }
        catch (ClientResultException exception)
        {
            logger.LogError(exception, "Failed to generate embedding for text");
            throw;
        }
    }

    private static MemoryEntry ToEntry(MemoryRecord record) =>
        new(record.Id, record.Content, record.Category, record.CreatedAt);
}