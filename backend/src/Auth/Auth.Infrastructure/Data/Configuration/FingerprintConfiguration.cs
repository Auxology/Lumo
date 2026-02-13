using Auth.Domain.ValueObjects;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using SharedKernel.Infrastructure.Data;

namespace Auth.Infrastructure.Data.Configuration;

internal static class FingerprintConfiguration
{
    public static void ConfigureFingerprint(this ComplexPropertyBuilder<Fingerprint> fp)
    {
        fp.Property(f => f.IpAddress)
            .IsRequired()
            .HasMaxLength(DataConfigurationConstants.MaxIpAddressLength)
            .HasColumnType(DataConfigurationConstants.DefaultStringColumnType);

        fp.Property(f => f.UserAgent)
            .IsRequired()
            .HasMaxLength(DataConfigurationConstants.MaxUserAgentLength)
            .HasColumnType(DataConfigurationConstants.DefaultStringColumnType);

        fp.Property(f => f.Timezone)
            .IsRequired()
            .HasMaxLength(DataConfigurationConstants.MaxTimezoneLength)
            .HasColumnType(DataConfigurationConstants.DefaultStringColumnType);

        fp.Property(f => f.Language)
            .IsRequired()
            .HasMaxLength(DataConfigurationConstants.MaxLanguageLength)
            .HasColumnType(DataConfigurationConstants.DefaultStringColumnType);

        fp.Property(f => f.ComputedHash)
            .IsRequired()
            .HasMaxLength(DataConfigurationConstants.DefaultStringMaxLength)
            .HasColumnType(DataConfigurationConstants.DefaultStringColumnType);

        fp.Property(f => f.NormalizedBrowser)
            .IsRequired()
            .HasMaxLength(DataConfigurationConstants.MaxNormalizedBrowserLength)
            .HasColumnType(DataConfigurationConstants.DefaultStringColumnType);

        fp.Property(f => f.NormalizedOs)
            .IsRequired()
            .HasMaxLength(DataConfigurationConstants.MaxNormalizedOsLength)
            .HasColumnType(DataConfigurationConstants.DefaultStringColumnType);
    }
}