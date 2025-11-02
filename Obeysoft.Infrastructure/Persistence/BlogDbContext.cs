using Microsoft.EntityFrameworkCore;
using Obeysoft.Domain.Categories;
using Obeysoft.Domain.Comments;
using Obeysoft.Domain.Posts;
using Obeysoft.Domain.Users;
using Obeysoft.Domain.Navigation;

namespace Obeysoft.Infrastructure.Persistence
{
    public sealed class BlogDbContext : DbContext
    {
        public BlogDbContext(DbContextOptions<BlogDbContext> options) : base(options) { }

        public DbSet<Category> Categories => Set<Category>();
        public DbSet<Post> Posts => Set<Post>();
        public DbSet<Comment> Comments => Set<Comment>();
        public DbSet<User> Users => Set<User>();
        public DbSet<UserProfile> UserProfiles => Set<UserProfile>();
        public DbSet<NavigationItem> NavigationItems => Set<NavigationItem>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Bütün IEntityTypeConfiguration<> sınıflarını bu assembly'den uygula
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(BlogDbContext).Assembly);

            // Varsayılan şema vs. ek ayarlar gerekirse burada
            base.OnModelCreating(modelBuilder);
        }
    }
}
