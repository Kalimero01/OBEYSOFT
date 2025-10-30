using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Obeysoft.Domain.Users;

namespace Obeysoft.Infrastructure.Persistence.Configurations
{
    internal sealed class UserConfiguration : IEntityTypeConfiguration<User>
    {
        public void Configure(EntityTypeBuilder<User> builder)
        {
            builder.ToTable("Users");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.Email)
                   .IsRequired()
                   .HasMaxLength(256);

            builder.HasIndex(x => x.Email)
                   .IsUnique();

            builder.Property(x => x.DisplayName)
                   .IsRequired()
                   .HasMaxLength(160);

            builder.Property(x => x.PasswordHash)
                   .IsRequired();

            builder.Property(x => x.PasswordSalt)
                   .IsRequired();

            builder.Property(x => x.Role)
                   .IsRequired()
                   .HasConversion<string>()      // enum → string
                   .HasMaxLength(32);

            builder.Property(x => x.IsActive)
                   .IsRequired();

            builder.Property(x => x.CreatedAt)
                   .IsRequired();

            builder.Property(x => x.UpdatedAt)
                   .IsRequired(false);

            builder.Property(x => x.LastLoginAt)
                   .IsRequired(false);
        }
    }
}