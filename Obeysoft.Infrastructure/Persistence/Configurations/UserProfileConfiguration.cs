using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Obeysoft.Domain.Users;

namespace Obeysoft.Infrastructure.Persistence.Configurations
{
    internal sealed class UserProfileConfiguration : IEntityTypeConfiguration<UserProfile>
    {
        public void Configure(EntityTypeBuilder<UserProfile> builder)
        {
            builder.ToTable("UserProfiles");
            builder.HasKey(x => x.Id);

            builder.Property(x => x.UserId).IsRequired();
            builder.Property(x => x.Age).IsRequired(false);
            builder.Property(x => x.Gender).HasMaxLength(32).IsRequired(false);
            builder.Property(x => x.City).HasMaxLength(100).IsRequired(false);
            builder.Property(x => x.AvatarUrl).HasMaxLength(1024).IsRequired(false);

            builder.Property(x => x.CreatedAt).IsRequired();
            builder.Property(x => x.UpdatedAt).IsRequired(false);

            builder.HasIndex(x => x.UserId).IsUnique();
        }
    }
}


