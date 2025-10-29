using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Obeysoft.Infrastructure.Persistence
{
    /// <summary>
    /// Development ortamında örnek kategori ve post verileri ekler.
    /// Domain setter'larına dokunmamak için ham SQL kullanır.
    /// </summary>
    public static class BlogDbSeeder
    {
        public static async Task SeedAsync(BlogDbContext db, ILogger logger)
        {
            // Şema/migration garanti
            await db.Database.MigrateAsync();

            logger.LogInformation("🌱 Seed başlıyor...");

            // Sabit GUID'ler (tekrar çalıştırıldığında çakışma yaşamamak için)
            var aspId = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaa1");
            var phpId = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaa2");
            var efcId = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaa3");

            var now = DateTimeOffset.UtcNow;

            // --- KATEGORİLER ---
            // ASP.NET Core (root)
            await db.Database.ExecuteSqlRawAsync(
                """
                INSERT INTO "Categories" ("Id","Name","Slug","Description","IsActive","DisplayOrder","ParentId","CreatedAt","UpdatedAt")
                VALUES ({0},{1},{2},{3},TRUE,1,NULL,{4},NULL)
                ON CONFLICT ("Slug") DO NOTHING;
                """,
                aspId, "ASP.NET Core", "aspnet-core",
                "ASP.NET Core dersleri ve rehberleri", now);

            // PHP (root)
            await db.Database.ExecuteSqlRawAsync(
                """
                INSERT INTO "Categories" ("Id","Name","Slug","Description","IsActive","DisplayOrder","ParentId","CreatedAt","UpdatedAt")
                VALUES ({0},{1},{2},{3},TRUE,2,NULL,{4},NULL)
                ON CONFLICT ("Slug") DO NOTHING;
                """,
                phpId, "PHP", "php",
                "PHP ve Laravel üzerine içerikler", now);

            // EF Core (ASP altında child)
            await db.Database.ExecuteSqlRawAsync(
                """
                INSERT INTO "Categories" ("Id","Name","Slug","Description","IsActive","DisplayOrder","ParentId","CreatedAt","UpdatedAt")
                VALUES ({0},{1},{2},{3},TRUE,3,{4},{5},NULL)
                ON CONFLICT ("Slug") DO NOTHING;
                """,
                efcId, "Entity Framework Core", "ef-core",
                "EF Core mimarisi, sorgular ve performans tüyoları", aspId, now);

            // --- POSTLAR ---
            var p1 = Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbb1");
            var p2 = Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbb2");

            await db.Database.ExecuteSqlRawAsync(
                """
                INSERT INTO "Posts" 
                ("Id","Title","Slug","Summary","Content","CategoryId","IsPublished","IsActive","IsDeleted","PublishedAt","CreatedAt","UpdatedAt")
                VALUES
                ({0},{1},{2},{3},{4},{5},TRUE,TRUE,FALSE,{6},{6},NULL)
                ON CONFLICT ("Slug") DO NOTHING;
                """,
                p1,
                "ASP.NET Core ile Katmanlı Mimari",
                "aspnet-core-ile-katmanli-mimari",
                "Domain, Application, Infrastructure ve API katmanlarını adım adım açıklıyoruz.",
                "Bu yazıda, profesyonel .NET 8 DDD yapısını temelden ele alacağız...",
                aspId,
                now
            );

            await db.Database.ExecuteSqlRawAsync(
                """
                INSERT INTO "Posts" 
                ("Id","Title","Slug","Summary","Content","CategoryId","IsPublished","IsActive","IsDeleted","PublishedAt","CreatedAt","UpdatedAt")
                VALUES
                ({0},{1},{2},{3},{4},{5},TRUE,TRUE,FALSE,{6},{6},NULL)
                ON CONFLICT ("Slug") DO NOTHING;
                """,
                p2,
                "Entity Framework Core Performans Tüyoları",
                "ef-core-performans-tuyolari",
                "Tracking, AsNoTracking ve sorgu optimizasyonu üzerine.",
                "EF Core performansını artırmak için dikkat edilmesi gereken noktaları açıklıyoruz...",
                efcId,
                now
            );

            logger.LogInformation("✅ Seed tamam.");
        }
    }
}
