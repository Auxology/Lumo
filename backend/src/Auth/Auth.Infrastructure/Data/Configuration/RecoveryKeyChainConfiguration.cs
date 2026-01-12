using Auth.Domain.Aggregates;
using Auth.Domain.ValueObjects;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Auth.Infrastructure.Data.Configuration;

internal sealed class RecoveryKeyChainConfiguration : IEntityTypeConfiguration<RecoveryKeyChain>
{
    public void Configure(EntityTypeBuilder<RecoveryKeyChain> b)
    {
        b.HasKey(rkc => rkc.Id);

        b.Property(rkc => rkc.Id)
            .ValueGeneratedNever()
            .HasConversion
            (
                id => id.Value,
                s => RecoveryKeyChainId.UnsafeFrom(s)
            )
            .HasColumnType($"varchar({RecoveryKeyChainId.Length})");

        b.Property(rkc => rkc.UserId)
            .IsRequired()
            .HasConversion
            (
                id => id.Value,
                guid => UserId.UnsafeFromGuid(guid)
            )
            .HasColumnType("uuid");

        b.Property(rkc => rkc.CreatedAt)
            .IsRequired()
            .HasColumnType("timestamptz");

        b.Property(rkc => rkc.LastRotatedAt)
            .IsRequired(false)
            .HasColumnType("timestamptz");

        b.Property(rkc => rkc.Version)
            .IsRequired()
            .IsConcurrencyToken();

        b.HasMany(rkc => rkc.RecoveryKeys)
            .WithOne()
            .HasForeignKey(rk => rk.RecoveryKeyChainId)
            .HasPrincipalKey(rkc => rkc.Id)
            .OnDelete(DeleteBehavior.Cascade);

        b.HasIndex(rkc => rkc.UserId)
            .IsUnique();

        b.HasIndex(rkc => rkc.CreatedAt);
    }
}