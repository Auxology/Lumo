using Main.Domain.Aggregates;
using Main.Domain.ValueObjects;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Main.Infrastructure.Data.Configuration;

internal sealed class PreferenceConfiguration : IEntityTypeConfiguration<Preference>
{
    public void Configure(EntityTypeBuilder<Preference> b)
    {
        b.HasKey(p => p.Id);

        b.Property(p => p.Id)
            .ValueGeneratedNever()
            .HasConversion
            (
                id => id.Value,
                s => PreferenceId.UnsafeFrom(s)
            )
            .HasColumnType($"varchar({PreferenceId.Length})");

        b.Property(p => p.UserId)
            .IsRequired()
            .HasColumnType("uuid");

        b.Property(p => p.CreatedAt)
            .IsRequired()
            .HasColumnType("timestamptz");

        b.Property(p => p.UpdatedAt)
            .IsRequired()
            .HasColumnType("timestamptz");

        b.HasMany(p => p.Instructions)
            .WithOne()
            .HasForeignKey(i => i.PreferenceId)
            .HasPrincipalKey(p => p.Id)
            .OnDelete(DeleteBehavior.Cascade);

        b.HasMany(p => p.FavoriteModels)
            .WithOne()
            .HasForeignKey(f => f.PreferenceId)
            .HasPrincipalKey(p => p.Id)
            .OnDelete(DeleteBehavior.Cascade);

        b.HasIndex(p => p.UserId)
            .IsUnique();
    }
}