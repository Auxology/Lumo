using Auth.Domain.Entities;
using Auth.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SharedKernel.Infrastructure.Data;

namespace Auth.Infrastructure.Data.Configuration;

internal sealed class RecoveryKeyConfiguration : IEntityTypeConfiguration<RecoveryKey>
{
    public void Configure(EntityTypeBuilder<RecoveryKey> b)
    {
        b.HasKey(rk => rk.Id);

        b.Property(rk => rk.RecoveryKeyChainId)
            .IsRequired()
            .HasConversion
            (
                id => id.Value,
                guid => RecoveryKeyChainId.UnsafeFromGuid(guid)
            )
            .HasColumnType("uuid");

        b.Property(rk => rk.Identifier)
            .IsRequired()
            .HasMaxLength(DataConfigurationConstants.DefaultStringMaxLength)
            .HasColumnType("varchar");

        b.Property(rk => rk.VerifierHash)
            .IsRequired()
            .HasMaxLength(DataConfigurationConstants.DefaultStringMaxLength)
            .HasColumnType("varchar");

        b.Property(rk => rk.IsUsed)
            .IsRequired()
            .HasColumnType("boolean");

        b.Property(rk => rk.UsedAt)
            .IsRequired(false)
            .HasColumnType("timestamptz");

        b.ComplexProperty(rk => rk.Fingerprint, fp =>
        {
            fp.Property(f => f.IpAddress)
                .IsRequired()
                .HasMaxLength(DataConfigurationConstants.MaxIpAddressLength)
                .HasColumnType("varchar");

            fp.Property(f => f.UserAgent)
                .IsRequired()
                .HasMaxLength(DataConfigurationConstants.MaxUserAgentLength)
                .HasColumnType("varchar");

            fp.Property(f => f.Timezone)
                .IsRequired()
                .HasMaxLength(DataConfigurationConstants.MaxTimezoneLength)
                .HasColumnType("varchar");

            fp.Property(f => f.Language)
                .IsRequired()
                .HasMaxLength(DataConfigurationConstants.MaxLanguageLength)
                .HasColumnType("varchar");

            fp.Property(f => f.ComputedHash)
                .IsRequired()
                .HasMaxLength(DataConfigurationConstants.DefaultStringMaxLength)
                .HasColumnType("varchar");

            fp.Property(f => f.NormalizedBrowser)
                .IsRequired()
                .HasMaxLength(DataConfigurationConstants.MaxNormalizedBrowserLength)
                .HasColumnType("varchar");

            fp.Property(f => f.NormalizedOs)
                .IsRequired()
                .HasMaxLength(DataConfigurationConstants.MaxNormalizedOsLength)
                .HasColumnType("varchar");
        });

        b.HasIndex(rk => rk.RecoveryKeyChainId);
        
        b.HasIndex(rk => rk.Identifier);
        
        b.HasIndex(rk => rk.IsUsed);
        
        b.HasIndex(rk => new { rk.RecoveryKeyChainId, rk.IsUsed });
        
        b.HasIndex(rk => new { rk.Identifier, rk.IsUsed });
    }
}

