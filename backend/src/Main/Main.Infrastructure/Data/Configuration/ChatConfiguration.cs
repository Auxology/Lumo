using Main.Domain.Aggregates;
using Main.Domain.Constants;
using Main.Domain.ValueObjects;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Main.Infrastructure.Data.Configuration;

internal sealed class ChatConfiguration : IEntityTypeConfiguration<Chat>
{
    public void Configure(EntityTypeBuilder<Chat> b)
    {
        b.HasKey(c => c.Id);

        b.Property(c => c.Id)
            .ValueGeneratedNever()
            .HasConversion
            (
                id => id.Value,
                s => ChatId.UnsafeFrom(s)
            )
            .HasColumnType($"varchar({ChatId.Length})");

        b.Property(c => c.UserId)
            .IsRequired()
            .HasColumnType("uuid");

        b.Property(c => c.Title)
            .IsRequired()
            .HasMaxLength(ChatConstants.MaxTitleLength)
            .HasColumnType($"varchar({ChatConstants.MaxTitleLength})");

        b.Property(c => c.ModelId)
            .IsRequired(false)
            .HasMaxLength(ChatConstants.MaxModelIdLength)
            .HasColumnType($"varchar({ChatConstants.MaxModelIdLength})");

        b.Property(c => c.IsArchived)
            .IsRequired()
            .HasColumnType("boolean");

        b.Property(c => c.CreatedAt)
            .IsRequired()
            .HasColumnType("timestamptz");

        b.Property(c => c.UpdatedAt)
            .IsRequired(false)
            .HasColumnType("timestamptz");

        b.HasMany(c => c.Messages)
            .WithOne()
            .HasForeignKey(m => m.ChatId)
            .HasPrincipalKey(c => c.Id)
            .OnDelete(DeleteBehavior.Cascade);

        b.HasIndex(c => new { c.UserId, c.IsArchived, c.UpdatedAt });
    }
}