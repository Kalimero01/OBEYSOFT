// FILE: Obeysoft.Infrastructure/Persistence/Configurations/PostConfiguration.cs
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Obeysoft.Domain.Categories;
using Obeysoft.Domain.Posts;

namespace Obeysoft.Infrastructure.Persistence.Configurations
{
    internal sealed class PostConfiguration : IEntityTypeConfiguration<Post>
    {
        public void Configure(EntityTypeBuilder<Post> builder)
        {
            builder.ToTable("Posts");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.Title)
                   .IsRequired()
                   .HasMaxLength(180);

            builder.Property(x => x.Slug)
                   .IsRequired()
                   .HasMaxLength(200);

            builder.Property(x => x.Summary)
                   .HasMaxLength(500);

            builder.Property(x => x.Content)
                   .IsRequired();

            builder.Property(x => x.IsPublished).IsRequired();
            builder.Property(x => x.IsActive).IsRequired();
            builder.Property(x => x.IsDeleted).IsRequired();

            builder.Property(x => x.CreatedAt).IsRequired();
            builder.Property(x => x.PublishedAt);
            builder.Property(x => x.UpdatedAt);

            builder.HasOne<Category>()
                   .WithMany()
                   .HasForeignKey(x => x.CategoryId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasIndex(x => x.Slug).IsUnique();

            builder.HasIndex(x => new { x.CategoryId, x.IsPublished, x.PublishedAt });
            builder.HasIndex(x => new { x.IsPublished, x.PublishedAt });
        }
    }
}