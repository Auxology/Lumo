using Main.Domain.ReadModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Main.Infrastructure.Data.Configuration;

internal sealed class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> b)
    {
        b.HasKey(u => u.UserId);

        b.Property(u => u.DisplayName)
            .IsRequired()
            .HasColumnType("varchar(1000)");

        b.Property(u => u.EmailAddress)
            .IsRequired()
            .HasColumnType("varchar(1000)");

        b.Property(u => u.AvatarKey)
            .IsRequired(false)
            .HasColumnType("varchar(1000)");

        b.HasIndex(u => u.EmailAddress)
            .IsUnique();
    }
}
