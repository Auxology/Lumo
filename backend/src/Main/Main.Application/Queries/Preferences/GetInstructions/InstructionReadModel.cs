namespace Main.Application.Queries.Preferences.GetInstructions;

public sealed record InstructionReadModel
{
    public required string Id { get; init; }

    public required string PreferenceId { get; init; }

    public required string Content { get; init; }

    public int Priority { get; init; }

    public DateTimeOffset CreatedAt { get; init; }

    public DateTimeOffset UpdatedAt { get; init; }
}