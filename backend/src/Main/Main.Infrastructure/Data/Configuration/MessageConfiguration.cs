using Main.Domain.Entities;
using Main.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SharedKernel.Infrastructure.Data;

namespace Main.Infrastructure.Data.Configuration;

internal sealed class MessageConfiguration : IEntityTypeConfiguration<Message>
{
    public void Configure(EntityTypeBuilder<Message> b)
    {
        b.HasKey(m => m.Id);

        b.Property(m => m.Id)
            .IsRequired()
            .ValueGeneratedOnAdd()
            .HasColumnType("bigint");

        b.Property(c => c.ChatId)
            .IsRequired()
            .HasConversion
            (
                id => id.Value,
                guid => ChatId.UnsafeFromGuid(guid)
            )
            .HasColumnType("uuid");

        b.Property(m => m.MessageRole)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(DataConfigurationConstants.DefaultStringMaxLength);

        b.Property(m => m.MessageContent)
            .IsRequired()
            .HasColumnType("text");

        b.Property(m => m.TokenCount)
            .IsRequired(false)
            .HasColumnType("bigint");

        b.Property(m => m.CreatedAt)
            .IsRequired()
            .HasColumnType("timestamptz");

        b.HasIndex(m => m.ChatId);

        b.HasIndex(m => new { m.ChatId, m.Id });
    }
}
