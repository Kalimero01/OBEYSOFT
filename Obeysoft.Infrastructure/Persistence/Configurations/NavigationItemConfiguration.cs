using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Obeysoft.Domain.Navigation;

namespace Obeysoft.Infrastructure.Persistence.Configurations
{
    internal sealed class NavigationItemConfiguration : IEntityTypeConfiguration<NavigationItem>
    {
        public void Configure(EntityTypeBuilder<NavigationItem> builder)
        {
            builder.ToTable("NavigationItems");
            builder.HasKey(x => x.Id);
            builder.Property(x => x.Label).HasMaxLength(160).IsRequired();
            builder.Property(x => x.Href).HasMaxLength(512).IsRequired();
            builder.Property(x => x.DisplayOrder).IsRequired();
            builder.Property(x => x.IsActive).IsRequired();
            builder.Property(x => x.CreatedAt).IsRequired();
            builder.Property(x => x.UpdatedAt).IsRequired(false);

            builder.HasIndex(x => new { x.ParentId, x.DisplayOrder, x.Label });
        }
    }
}



