using Main.Application.Abstractions.Memory;

using Pgvector;

namespace Main.Infrastructure.Data.Entities;

internal sealed class MemoryRecord
{
    public required string Id { get; init; }

    public required Guid UserId { get; init; }

    public required string Content { get; init; }

    public required MemoryCategory Category { get; init; }

    public required Vector Embedding { get; init; }

    public required DateTimeOffset CreatedAt { get; init; }
}