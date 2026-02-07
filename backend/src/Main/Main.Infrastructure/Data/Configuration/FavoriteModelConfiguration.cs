using Main.Domain.Constants;
using Main.Domain.Entities;
using Main.Domain.ValueObjects;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using SharedKernel.Infrastructure.Data;

namespace Main.Infrastructure.Data.Configuration;

internal sealed class FavoriteModelConfiguration : IEntityTypeConfiguration<FavoriteModel>
{
    public void Configure(EntityTypeBuilder<FavoriteModel> b)
    {
        b.HasKey(f => f.Id);

        b.Property(f => f.Id)
            .ValueGeneratedNever()
            .HasConversion
            (
                id => id.Value,
                s => FavoriteModelId.UnsafeFrom(s)
            )
            .HasColumnType($"{DataConfigurationConstants.DefaultStringColumnType}({FavoriteModelId.Length})");

        b.Property(f => f.PreferenceId)
            .ValueGeneratedNever()
            .HasConversion
            (
                id => id.Value,
                s => PreferenceId.UnsafeFrom(s)
            )
            .HasColumnType($"{DataConfigurationConstants.DefaultStringColumnType}({PreferenceId.Length})");

        b.Property(f => f.ModelId)
            .IsRequired()
            .HasMaxLength(FavoriteModelConstants.MaxModelIdLength)
            .HasColumnType($"{DataConfigurationConstants.DefaultStringColumnType}({FavoriteModelConstants.MaxModelIdLength})");

        b.Property(f => f.CreatedAt)
            .IsRequired()
            .HasColumnType(DataConfigurationConstants.DefaultTimeColumnType);

        b.HasIndex(f => f.PreferenceId);

        b.HasIndex(f => new { f.PreferenceId, f.ModelId })
            .IsUnique();
    }
}