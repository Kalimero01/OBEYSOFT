// FILE: Obeysoft.Infrastructure/DependencyInjection.cs
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Obeysoft.Infrastructure.Persistence;

// Domain arayüzleri
using Obeysoft.Domain.Comments;

// Infrastructure implementasyonları
using Obeysoft.Infrastructure.Comments;

// Application arayüz/servisleri
using Obeysoft.Application.Auth;
using Obeysoft.Application.Posts;
using Obeysoft.Application.Categories;
using Obeysoft.Application.Comments;

// Infrastructure’daki servis implementasyonları
using Obeysoft.Infrastructure.Auth;
using Obeysoft.Infrastructure.Posts;
using Obeysoft.Infrastructure.Categories;

namespace Obeysoft.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            // ---- DbContext (PostgreSQL) ----
            var conn = configuration.GetConnectionString("DefaultConnection")
                       ?? "Host=127.0.0.1;Port=5432;Database=obeysoft_dev;Username=postgres;Password=postgres";

            services.AddDbContext<BlogDbContext>(opt =>
            {
                opt.UseNpgsql(conn, npg =>
                {
                    npg.MigrationsAssembly(typeof(BlogDbContext).Assembly.FullName);
                });
            });

            // ---- Repositories ----
            services.AddScoped<ICommentRepository, CommentRepository>();

            // ---- Services ----
            // Auth
            services.AddScoped<IAuthService, AuthService>();

            // Post okumalar
            services.AddScoped<IGetPostService, GetPostService>();
            // Post yönetimi (YENİ)
            services.AddScoped<IManagePostService, ManagePostService>();

            // Category
            services.AddScoped<IGetCategoryService, GetCategoryService>();

            // Comments
            services.AddScoped<ICreateCommentService, CreateCommentService>();

            return services;
        }
    }
}
