// FILE: Obeysoft.Infrastructure/Persistence/BlogDbContextFactory.cs
using System;
using System.IO;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration; // 🔹 eksik using buydu!

namespace Obeysoft.Infrastructure.Persistence
{
    /// <summary>
    /// EF Core tasarım zamanında (migrations) BlogDbContext oluşturmak için factory.
    /// Uygulamanın DI'ını başlatmadan context yaratır; böylece Validator vb. bağımlılıklar gerekmez.
    /// </summary>
    public sealed class BlogDbContextFactory : IDesignTimeDbContextFactory<BlogDbContext>
    {
        public BlogDbContext CreateDbContext(string[] args)
        {
            var basePath = Directory.GetCurrentDirectory();
            var env = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Development";

            // appsettings’i bulmak için hem kendi dizinini hem de API projesini dene
            var builder = new ConfigurationBuilder()
                .SetBasePath(basePath)
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: false)
                .AddJsonFile($"appsettings.{env}.json", optional: true, reloadOnChange: false);

            var apiPath = Path.Combine(basePath, "..", "Obeysoft.Api");
            if (Directory.Exists(apiPath))
            {
                builder.AddJsonFile(Path.Combine(apiPath, "appsettings.json"), optional: true, reloadOnChange: false)
                       .AddJsonFile(Path.Combine(apiPath, $"appsettings.{env}.json"), optional: true, reloadOnChange: false);
            }

            builder.AddEnvironmentVariables();

            var config = builder.Build();

            var conn = config.GetConnectionString("DefaultConnection")
                       ?? "Host=127.0.0.1;Port=5432;Database=obeysoft_dev;Username=postgres;Password=postgres";

            var options = new DbContextOptionsBuilder<BlogDbContext>()
                .UseNpgsql(conn, npg =>
                {
                    npg.MigrationsAssembly(typeof(BlogDbContext).Assembly.FullName);
                })
                .Options;

            return new BlogDbContext(options);
        }
    }
}
