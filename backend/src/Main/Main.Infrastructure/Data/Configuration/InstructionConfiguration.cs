using Main.Domain.Constants;
using Main.Domain.Entities;
using Main.Domain.ValueObjects;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using SharedKernel.Infrastructure.Data;

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
            .HasColumnType($"{DataConfigurationConstants.DefaultStringColumnType}({InstructionId.Length})");

        b.Property(i => i.PreferenceId)
            .ValueGeneratedNever()
            .HasConversion
            (
                id => id.Value,
                s => PreferenceId.UnsafeFrom(s)
            )
            .HasColumnType($"{DataConfigurationConstants.DefaultStringColumnType}({PreferenceId.Length})");

        b.Property(i => i.Content)
            .IsRequired()
            .HasMaxLength(InstructionConstants.MaxContentLength)
            .HasColumnType($"{DataConfigurationConstants.DefaultStringColumnType}({InstructionConstants.MaxContentLength})");

        b.Property(i => i.Priority)
            .IsRequired()
            .HasColumnType("integer");

        b.Property(i => i.CreatedAt)
            .IsRequired()
            .HasColumnType(DataConfigurationConstants.DefaultTimeColumnType);

        b.Property(i => i.UpdatedAt)
            .IsRequired()
            .HasColumnType(DataConfigurationConstants.DefaultTimeColumnType);

        b.HasIndex(i => i.PreferenceId);

        b.HasIndex(i => new { i.PreferenceId, i.Priority })
            .IsUnique();
    }
}