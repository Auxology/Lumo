using Auth.Domain.Aggregates.SessionAggregate;
using Auth.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Auth.Infrastructure.Data.Configurations;

internal sealed class SessionConfiguration : IEntityTypeConfiguration<Session>
{
      public void Configure(EntityTypeBuilder<Session> builder)
      {
          builder.HasKey(x => x.Id);

          builder.Property(x => x.Id)
              .HasConversion(
                  id => id.Value,
                  value => SessionId.UnsafeFromGuid(value))
              .HasColumnType("uuid")
              .IsRequired();

          builder.Property(x => x.UserId)
              .HasConversion(
                  id => id.Value,
                  value => UserId.UnsafeFromGuid(value))
              .HasColumnType("uuid")
              .IsRequired();

          builder.Property(x => x.HashedRefreshToken)
              .HasMaxLength(AuthDbConstants.MaxUnenforcedLength)
              .IsRequired();

          builder.Property(x => x.IpAddress)
              .HasMaxLength(AuthDbConstants.MaxIpAddressLength)
              .IsRequired();

          builder.Property(x => x.UserAgent)
              .HasMaxLength(AuthDbConstants.MaxUnenforcedLength)
              .IsRequired();

          builder.Property(x => x.Version)
              .IsRequired();

          builder.Property(x => x.CreatedAt)
              .HasColumnType("timestamptz")
              .IsRequired();

          builder.Property(s => s.ExpiresAt)
              .HasColumnType("timestamptz")
              .IsRequired();

          builder.Property(x => x.AbsoluteExpiresAt)
              .HasColumnType("timestamptz")
              .IsRequired();

          builder.Property(x => x.RevokedAt)
              .HasColumnType("timestamptz")
              .IsRequired(false);

          builder.Property(x => x.LastRefreshedAt)
              .HasColumnType("timestamptz")
              .IsRequired(false);

          builder.HasIndex(x => x.UserId);

          builder.HasIndex(x => new { x.UserId, x.RevokedAt, x.ExpiresAt });

          builder.HasIndex(x => x.CreatedAt);

          builder.Ignore(x => x.DomainEvents);
      }
}