using System.Diagnostics.CodeAnalysis;

using Main.Domain.Faults;
using Main.Domain.ValueObjects;

using SharedKernel;

namespace Main.Domain.Entities;

public sealed class FavoriteModel : Entity<FavoriteModelId>
{
    public PreferenceId PreferenceId { get; private set; }

    public string ModelId { get; private set; } = string.Empty;

    public DateTimeOffset CreatedAt { get; private set; }

    private FavoriteModel() { } // For EF Core

    [SetsRequiredMembers]
    private FavoriteModel
    (
        FavoriteModelId id,
        PreferenceId preferenceId,
        string modelId,
        DateTimeOffset utcNow
    )
    {
        Id = id;
        PreferenceId = preferenceId;
        ModelId = modelId;
        CreatedAt = utcNow;
    }

    public static Outcome<FavoriteModel> Create
    (
        FavoriteModelId id,
        PreferenceId preferenceId,
        string modelId,
        DateTimeOffset utcNow
    )
    {
        if (id.IsEmpty)
            return FavoriteModelFaults.FavoriteModelIdRequired;

        if (preferenceId.IsEmpty)
            return FavoriteModelFaults.PreferenceIdRequired;

        if (string.IsNullOrWhiteSpace(modelId))
            return FavoriteModelFaults.ModelIdRequired;

        FavoriteModel favoriteModel = new
        (
            id: id,
            preferenceId: preferenceId,
            modelId: modelId,
            utcNow: utcNow
        );

        return favoriteModel;
    }
}