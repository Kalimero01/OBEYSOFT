// FILE: Obeysoft.Infrastructure/Persistence/Configurations/CommentConfiguration.cs
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Obeysoft.Domain.Comments;
using Obeysoft.Domain.Posts;

namespace Obeysoft.Infrastructure.Persistence.Configurations
{
    internal sealed class CommentConfiguration : IEntityTypeConfiguration<Comment>
    {
        public void Configure(EntityTypeBuilder<Comment> builder)
        {
            builder.ToTable("Comments");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.Content)
                   .IsRequired()
                   .HasMaxLength(4000);

            builder.Property(x => x.IsActive).IsRequired();
            builder.Property(x => x.IsApproved).IsRequired();
            builder.Property(x => x.IsDeleted).IsRequired();

            builder.Property(x => x.CreatedAt).IsRequired();
            builder.Property(x => x.UpdatedAt);

            // Parent → children
            builder.HasOne<Comment>()
                   .WithMany()
                   .HasForeignKey(x => x.ParentId)
                   .OnDelete(DeleteBehavior.Restrict);

            // Post → Comments
            builder.HasOne<Post>()
                   .WithMany()
                   .HasForeignKey(x => x.PostId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasIndex(x => new { x.PostId, x.IsApproved, x.IsActive, x.CreatedAt });
        }
    }
}
