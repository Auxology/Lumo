using Main.Domain.Constants;
using Main.Domain.Entities;
using Main.Domain.ValueObjects;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Main.Infrastructure.Data.Configuration;

internal sealed class InstructionConfiguration : IEntityTypeConfiguration<Instruction>
{
    public void Configure(EntityTypeBuilder<Instruction> b)
    {
        b.HasKey(i => i.Id);

        b.Property(i => i.Id)
            .ValueGeneratedNever()
            .HasConversion
            (
                id => id.Value,
                s => InstructionId.UnsafeFrom(s)
            )
            .HasColumnType($"varchar({InstructionId.Length})");

        b.Property(i => i.PreferenceId)
            .ValueGeneratedNever()
            .HasConversion
            (
                id => id.Value,
                s => PreferenceId.UnsafeFrom(s)
            )
            .HasColumnType($"varchar({PreferenceId.Length})");

        b.Property(i => i.Content)
            .IsRequired()
            .HasMaxLength(InstructionConstants.MaxContentLength)
            .HasColumnType($"varchar({InstructionConstants.MaxContentLength})");

        b.Property(i => i.Priority)
            .IsRequired()
            .HasColumnType("integer");

        b.Property(i => i.CreatedAt)
            .IsRequired()
            .HasColumnType("timestamptz");

        b.Property(i => i.UpdatedAt)
            .IsRequired()
            .HasColumnType("timestamptz");

        b.HasIndex(i => i.PreferenceId);

        b.HasIndex(i => new { i.PreferenceId, i.Priority })
            .IsUnique();
    }
}