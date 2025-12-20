using Auth.Domain.Aggregates;
using Auth.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SharedKernel.Infrastructure.Data;

namespace Auth.Infrastructure.Data.Configuration;

internal sealed class LoginRequestConfiguration : IEntityTypeConfiguration<LoginRequest>
{
    public void Configure(EntityTypeBuilder<LoginRequest> b)
    {
        b.HasKey(lr => lr.Id);

        b.Property(lr => lr.Id)
            .HasConversion
            (
                id => id.Value,
                guid => LoginRequestId.UnsafeFromGuid(guid)
            )
            .HasColumnType("uuid");
        
        b.Property(lr => lr.UserId)
            .IsRequired()
            .HasConversion
            (
                id => id.Value,
                guid => UserId.UnsafeFromGuid(guid)
            )
            .HasColumnType("uuid");

        b.Property(lr => lr.TokenKey)
            .IsRequired()
            .HasMaxLength(DataConfigurationConstants.DefaultStringMaxLength)
            .HasColumnType("varchar");
        
        b.Property(lr => lr.OtpTokenHash)
            .IsRequired()
            .HasMaxLength(DataConfigurationConstants.DefaultStringMaxLength)
            .HasColumnType("varchar");
        
        b.Property(lr => lr.MagicLinkTokenHash)
            .IsRequired()
            .HasMaxLength(DataConfigurationConstants.DefaultStringMaxLength)
            .HasColumnType("varchar");
        
        b.ComplexProperty(lr => lr.Fingerprint, fp =>
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
        
        b.Property(lr => lr.CreatedAt)
            .IsRequired()
            .HasColumnType("timestamptz");
        
        b.Property(lr => lr.ExpiresAt)
            .IsRequired()
            .HasColumnType("timestamptz");

        b.Property(lr => lr.ConsumedAt)
            .IsRequired(false)
            .HasColumnType("timestamptz");

        b.HasIndex(lr => lr.UserId);
        
        b.HasIndex(lr => lr.TokenKey)
            .IsUnique();
        
        b.HasIndex(lr => lr.ExpiresAt);
        
        b.HasIndex(lr => lr.ConsumedAt);
        
        b.HasIndex(lr => new { lr.TokenKey, lr.ConsumedAt, lr.ExpiresAt });
    }
}