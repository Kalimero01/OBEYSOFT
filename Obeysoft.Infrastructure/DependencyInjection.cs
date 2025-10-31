// FILE: Obeysoft.Infrastructure/DependencyInjection.cs
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Obeysoft.Infrastructure.Persistence;
using System.Text.RegularExpressions;
using Npgsql;

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
            var conn = ResolveConnectionString(configuration.GetConnectionString("DefaultConnection"));

            if (string.IsNullOrWhiteSpace(conn))
            {
                conn = BuildConnectionStringFromEnv()
                       ?? "Host=127.0.0.1;Port=5432;Database=obeysoft_dev;Username=postgres;Password=postgres";
            }

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

        private static string? ResolveConnectionString(string? raw)
        {
            if (string.IsNullOrWhiteSpace(raw))
            {
                return raw;
            }

            return Regex.Replace(raw, "\\$\\{([^}]+)\\}", match =>
            {
                var key = match.Groups[1].Value;
                var value = Environment.GetEnvironmentVariable(key);
                if (string.IsNullOrWhiteSpace(value))
                {
                    throw new InvalidOperationException($"Environment variable '{key}' is required to expand the database connection string but was not found.");
                }

                return value;
            });
        }

        private static string? BuildConnectionStringFromEnv()
        {
            var fromUrl = TryBuildFromDatabaseUrl();
            if (!string.IsNullOrWhiteSpace(fromUrl))
            {
                return fromUrl;
            }

            var host = Environment.GetEnvironmentVariable("DB_HOST");
            var database = Environment.GetEnvironmentVariable("DB_NAME");
            var user = Environment.GetEnvironmentVariable("DB_USER");
            var password = Environment.GetEnvironmentVariable("DB_PASSWORD");

            if (string.IsNullOrWhiteSpace(host) ||
                string.IsNullOrWhiteSpace(database) ||
                string.IsNullOrWhiteSpace(user) ||
                string.IsNullOrWhiteSpace(password))
            {
                return null;
            }

            var port = Environment.GetEnvironmentVariable("DB_PORT");
            var sslMode = Environment.GetEnvironmentVariable("DB_SSLMODE");
            var trustServerCert = Environment.GetEnvironmentVariable("DB_TRUST_SERVER_CERTIFICATE");

            port = string.IsNullOrWhiteSpace(port) ? "5432" : port;
            sslMode = string.IsNullOrWhiteSpace(sslMode) ? "Require" : sslMode;
            trustServerCert = string.IsNullOrWhiteSpace(trustServerCert) ? "true" : trustServerCert;

            return $"Host={host};Port={port};Database={database};Username={user};Password={password};SSL Mode={sslMode};Trust Server Certificate={trustServerCert}";
        }

        private static string? TryBuildFromDatabaseUrl()
        {
            var url = Environment.GetEnvironmentVariable("DATABASE_URL")
                      ?? Environment.GetEnvironmentVariable("DATABASE_URL_INTERNAL");

            if (string.IsNullOrWhiteSpace(url))
            {
                return null;
            }

            try
            {
                var builder = new NpgsqlConnectionStringBuilder(url);

                if (builder.SslMode == SslMode.Disable)
                {
                    builder.SslMode = SslMode.Require;
                }

                var connection = builder.ToString();

                if (!connection.Contains("Trust Server Certificate=", StringComparison.OrdinalIgnoreCase))
                {
                    var trustValue = Environment.GetEnvironmentVariable("DB_TRUST_SERVER_CERTIFICATE");
                    trustValue = string.IsNullOrWhiteSpace(trustValue) ? "true" : trustValue;
                    connection += $";Trust Server Certificate={trustValue}";
                }

                return connection;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("DATABASE_URL could not be parsed into a valid PostgreSQL connection string.", ex);
            }
        }
    }
}
