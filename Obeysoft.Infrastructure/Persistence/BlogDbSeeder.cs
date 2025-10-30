// FILE: Obeysoft.Infrastructure/Persistence/BlogDbSeeder.cs
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Obeysoft.Domain.Categories;
using Obeysoft.Domain.Comments;
using Obeysoft.Domain.Posts;
using Obeysoft.Domain.Users;

namespace Obeysoft.Infrastructure.Persistence
{
    /// <summary>
    /// Uygulama açılırken (Program.cs içinde) çağrılır.
    /// Amaç: Boş veritabanını ilk demo verileriyle doldurmak.
    /// Domain entity'lerinin PRIVATE ctor'larına takılmamak için
    /// HER ŞEYİ domain'in kendi factory metotlarıyla oluşturuyoruz.
    /// </summary>
    public static class BlogDbSeeder
    {
        public static async Task SeedAsync(BlogDbContext db, ILogger logger)
        {
            // 1) DB ayakta mı?
            await db.Database.EnsureCreatedAsync();

            // -------------------------------------------------
            // 1) KULLANICI
            // -------------------------------------------------
            // Domain.User'da public ctor yok, sadece factory var:
            // User.CreateNew(string email, string displayName, UserRole role = Member, bool isActive = true)
            User? adminUser = await db.Users.FirstOrDefaultAsync(u => u.Email == "admin@obeysoft.local");
            if (adminUser is null)
            {
                adminUser = User.CreateNew(
                    email: "admin@obeysoft.local",
                    displayName: "Super Admin",
                    role: UserRole.Admin,
                    isActive: true
                );

                db.Users.Add(adminUser);
                logger.LogInformation("Seed → admin@obeysoft.local eklendi (rol=Admin).");
            }

            // -------------------------------------------------
            // 2) KATEGORİLER
            // -------------------------------------------------
            // Domain.Category'de de public ctor yok;
            // Category.CreateRoot(...) ve Category.CreateChild(...) var.
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
            // Domain.Post’ta da public ctor yok;
            // Post.CreateDraft(string title, string slug, string content, Guid categoryId, string? summary = null, bool isActive = true)
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

                // Bu ikisini direkt yayımlayalım
                post1.Publish();
                post2.Publish();

                db.Posts.AddRange(post1, post2);
                logger.LogInformation("Seed → 2 post eklendi ve publish edildi.");

                // -------------------------------------------------
                // 4) YORUM
                // -------------------------------------------------
                // Domain.Comment: Comment.Create(Guid postId, Guid authorId, string content, Guid? parentId = null, bool isActive = true)
                // Burada authorId olarak biraz önce oluşturduğumuz adminUser.Id'yi kullanıyoruz.
                var comment = Comment.Create(
                    postId: post1.Id,
                    authorId: adminUser.Id,
                    content: "Canlıda ilk yorum 👋",
                    parentId: null,
                    isActive: true
                );
                // Yorumlar varsayılan olarak IsApproved=false geliyor;
                // ama seeder'da direkt onaylı olsun:
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
    }
}