// FILE: Obeysoft.Infrastructure/Persistence/Configurations/CategoryConfiguration.cs
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Obeysoft.Domain.Categories;

namespace Obeysoft.Infrastructure.Persistence.Configurations
{
    internal sealed class CategoryConfiguration : IEntityTypeConfiguration<Category>
    {
        public void Configure(EntityTypeBuilder<Category> builder)
        {
            builder.ToTable("Categories");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.Name)
                   .IsRequired()
                   .HasMaxLength(160);

            builder.Property(x => x.Slug)
                   .IsRequired()
                   .HasMaxLength(180);

            builder.Property(x => x.Description)
                   .HasMaxLength(1000);

            builder.Property(x => x.IsActive).IsRequired();
            builder.Property(x => x.DisplayOrder).IsRequired();

            builder.Property(x => x.CreatedAt).IsRequired();
            builder.Property(x => x.UpdatedAt).IsRequired(false);

            // Self reference (Parent → Children)
            builder.HasOne<Category>()
                   .WithMany()
                   .HasForeignKey(x => x.ParentId)
                   .OnDelete(DeleteBehavior.Restrict);

            // Slug benzersiz
            builder.HasIndex(x => x.Slug).IsUnique();

            // Menü sıralama indeksleri
            builder.HasIndex(x => new { x.ParentId, x.DisplayOrder, x.Name });
        }
    }
}
