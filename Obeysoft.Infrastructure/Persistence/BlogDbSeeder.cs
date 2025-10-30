// FILE: Obeysoft.Infrastructure/Persistence/BlogDbSeeder.cs
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Obeysoft.Domain.Categories;
using Obeysoft.Domain.Comments;
using Obeysoft.Domain.Posts;
using Obeysoft.Domain.Users;
using System;
using System.Security.Cryptography;

namespace Obeysoft.Infrastructure.Persistence
{
    /// <summary>
    /// Uygulama açılırken (Program.cs içinde) çağrılır.
    /// Amaç: Boş veritabanını ilk demo verileriyle doldurmak.
    /// - Admin bilgisi önce ortam değişkenlerinden okunur:
    ///     OBEYSOFT_ADMIN_EMAIL
    ///     OBEYSOFT_ADMIN_PASSWORD
    ///     OBEYSOFT_ADMIN_DISPLAYNAME
    /// - Yoksa varsayılan admin: admin@obeysoft.local / Admin1234!
    /// </summary>
    public static class BlogDbSeeder
    {
        public static async Task SeedAsync(BlogDbContext db, ILogger logger)
        {
            // 1) DB ayakta mı?
            await db.Database.EnsureCreatedAsync();

            // -------------------------------------------------
            // 0) Ortamdan admin bilgilerini oku
            // -------------------------------------------------
            var envAdminEmail = Environment.GetEnvironmentVariable("OBEYSOFT_ADMIN_EMAIL");
            var envAdminPassword = Environment.GetEnvironmentVariable("OBEYSOFT_ADMIN_PASSWORD");
            var envAdminDisplayName = Environment.GetEnvironmentVariable("OBEYSOFT_ADMIN_DISPLAYNAME");

            var adminEmail = string.IsNullOrWhiteSpace(envAdminEmail)
                ? "admin@obeysoft.local"
                : envAdminEmail.Trim().ToLowerInvariant();

            var adminDisplayName = string.IsNullOrWhiteSpace(envAdminDisplayName)
                ? "Super Admin"
                : envAdminDisplayName;

            var adminPassword = string.IsNullOrWhiteSpace(envAdminPassword)
                ? "Admin1234!"
                : envAdminPassword;

            // -------------------------------------------------
            // 1) KULLANICI
            // -------------------------------------------------
            var adminUser = await db.Users.FirstOrDefaultAsync(u => u.Email == adminEmail);
            if (adminUser is null)
            {
                // DDD'ye uygun: factory ile yarat
                adminUser = User.CreateNew(
                    email: adminEmail,
                    displayName: adminDisplayName,
                    role: UserRole.Admin,
                    isActive: true
                );

                // şifre üret (AuthService'tekiyle aynı mantık)
                var (hash, salt) = HashPassword(adminPassword);
                adminUser.SetPasswordSecret(hash, salt);

                db.Users.Add(adminUser);
                logger.LogInformation("Seed → {Email} eklendi (rol=Admin).", adminEmail);
            }
            else
            {
                logger.LogInformation("Seed → {Email} zaten var, yeniden eklenmedi.", adminEmail);
            }

            // -------------------------------------------------
            // 2) KATEGORİLER
            // -------------------------------------------------
            if (!await db.Categories.AnyAsync())
            {
                var catRoot = Category.CreateRoot(
                    name: "ASP.NET Core",
                    slug: "aspnet-core",
                    description: "ASP.NET Core dersleri",
                    displayOrder: 1,
                    isActive: true
                );

                var catPhp = Category.CreateRoot(
                    name: "PHP",
                    slug: "php",
                    description: "PHP ve Laravel",
                    displayOrder: 2,
                    isActive: true
                );

                var catEfCore = Category.CreateChild(
                    parentId: catRoot.Id,
                    name: "EF Core",
                    slug: "ef-core",
                    description: "EF Core mimarisi",
                    displayOrder: 1,
                    isActive: true
                );

                db.Categories.AddRange(catRoot, catPhp, catEfCore);
                logger.LogInformation("Seed → 3 kategori eklendi (ASP.NET Core, PHP, EF Core).");
            }

            await db.SaveChangesAsync(); // kategorilerin Id’leri kesinleşsin

            // Kategori id’lerini tekrar okuyalım
            var anyCategory = await db.Categories.OrderBy(c => c.DisplayOrder).FirstAsync();
            var aspnetCoreCat = await db.Categories.FirstOrDefaultAsync(c => c.Slug == "aspnet-core") ?? anyCategory;

            // -------------------------------------------------
            // 3) POSTLAR
            // -------------------------------------------------
            if (!await db.Posts.AnyAsync())
            {
                var post1 = Post.CreateDraft(
                    title: "Obeysoft'e Hoş Geldin",
                    slug: "obeysofte-hos-geldin",
                    content: "Bu, Render ortamında çalışan ilk yazın. Tebrikler 🎉",
                    categoryId: aspnetCoreCat.Id,
                    summary: "İlk canlı yazı.",
                    isActive: true
                );

                var post2 = Post.CreateDraft(
                    title: ".NET 8 + PostgreSQL + Render Yayında",
                    slug: "dotnet8-postgresql-render-yayinda",
                    content: "Bu yazı sadece canlı ortamı test etmek için eklendi.",
                    categoryId: aspnetCoreCat.Id,
                    summary: "Canlı test yazısı.",
                    isActive: true
                );

                // direkt yayımlayalım
                post1.Publish();
                post2.Publish();

                db.Posts.AddRange(post1, post2);
                logger.LogInformation("Seed → 2 post eklendi ve publish edildi.");

                // -------------------------------------------------
                // 4) YORUM
                // -------------------------------------------------
                var comment = Comment.Create(
                    postId: post1.Id,
                    authorId: adminUser!.Id,
                    content: "Canlıda ilk yorum 👋",
                    parentId: null,
                    isActive: true
                );
                comment.Approve();

                db.Comments.Add(comment);
                logger.LogInformation("Seed → 1 yorum eklendi.");
            }

            // -------------------------------------------------
            // 5) KAYDET
            // -------------------------------------------------
            await db.SaveChangesAsync();
            logger.LogInformation("Seed → tamamlandı.");
        }

        // AuthService'teki private PasswordHasher'ın kopyası (min)
        private static (string hash, string salt) HashPassword(string password)
        {
            if (string.IsNullOrWhiteSpace(password))
                throw new ArgumentException("Parola boş olamaz.", nameof(password));

            const int SaltSize = 32;
            const int HashSize = 32;
            const int Iterations = 100_000;

            Span<byte> salt = stackalloc byte[SaltSize];
            RandomNumberGenerator.Fill(salt);

            using var pbkdf2 = new Rfc2898DeriveBytes(password, salt.ToArray(), Iterations, HashAlgorithmName.SHA256);
            var hash = pbkdf2.GetBytes(HashSize);

            return (Convert.ToBase64String(hash), Convert.ToBase64String(salt.ToArray()));
        }
    }
}
