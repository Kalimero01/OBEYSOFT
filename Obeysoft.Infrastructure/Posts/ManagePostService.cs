// FILE: Obeysoft.Infrastructure/Posts/ManagePostService.cs
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Npgsql;
using Obeysoft.Application.Posts;
using Obeysoft.Domain.Posts;
using Obeysoft.Infrastructure.Persistence;

namespace Obeysoft.Infrastructure.Posts
{
    /// <summary>
    /// IManagePostService'in EF Core implementasyonu.
    /// Bu sınıf doğrudan PostgreSQL'e yazar.
    /// </summary>
    public sealed class ManagePostService : IManagePostService
    {
        private readonly BlogDbContext _db;
        private readonly ILogger<ManagePostService> _logger;

        public ManagePostService(BlogDbContext db, ILogger<ManagePostService> logger)
        {
            _db = db;
            _logger = logger;
        }

        // ------------------------------------------------------------
        // CREATE
        // ------------------------------------------------------------
        public async Task<PostSavedDto> CreateAsync(CreatePostRequestDto request, Guid actorUserId, CancellationToken ct)
        {
            // 1) Kategori gerçekten var mı ve aktif mi?
            var categoryOk = await _db.Categories
                .AsNoTracking()
                .AnyAsync(c => c.Id == request.CategoryId && c.IsActive, ct); // Category'de IsDeleted yok, sadece IsActive var. :contentReference[oaicite:7]{index=7}
            if (!categoryOk)
                throw new InvalidOperationException("Seçilen kategori bulunamadı veya pasif.");

            // 2) Slug hazır mı?
            var slug = !string.IsNullOrWhiteSpace(request.Slug)
                ? NormalizeSlug(request.Slug)
                : GenerateSlugFromTitle(request.Title);

            // 3) Slug çakışıyor mu?
            var slugInUse = await _db.Posts.AnyAsync(p => p.Slug == slug && !p.IsDeleted, ct);
            if (slugInUse)
                throw new InvalidOperationException("Bu slug zaten kullanılıyor.");

            // 4) Domain entity oluştur
            var post = Post.CreateDraft(
                title: request.Title,
                slug: slug,
                content: request.Content,
                categoryId: request.CategoryId,
                summary: request.Summary,
                isActive: true);

            // 5) Hemen yayınlansın mı?
            if (request.PublishNow)
            {
                post.Publish();
            }

            _db.Posts.Add(post);

            try
            {
                await _db.SaveChangesAsync(ct);
            }
            catch (PostgresException pex) when (pex.SqlState == "23505")
            {
                // unique violation
                _logger.LogWarning(pex, "Post oluşturulurken unique ihlali.");
                throw new InvalidOperationException("Bu başlık veya slug zaten kullanılıyor.");
            }

            return new PostSavedDto
            {
                Id = post.Id,
                Title = post.Title,
                Slug = post.Slug,
                CategoryId = post.CategoryId,
                IsPublished = post.IsPublished,
                PublishedAt = post.PublishedAt
            };
        }

        // ------------------------------------------------------------
        // UPDATE
        // ------------------------------------------------------------
        public async Task<PostSavedDto> UpdateAsync(UpdatePostRequestDto request, Guid actorUserId, CancellationToken ct)
        {
            var post = await _db.Posts.FirstOrDefaultAsync(p => p.Id == request.Id && !p.IsDeleted, ct);
            if (post is null)
                throw new InvalidOperationException("Güncellenecek post bulunamadı.");

            // kategori kontrolü
            var categoryOk = await _db.Categories
                .AsNoTracking()
                .AnyAsync(c => c.Id == request.CategoryId && c.IsActive, ct);
            if (!categoryOk)
                throw new InvalidOperationException("Seçilen kategori bulunamadı veya pasif.");

            // slug üret / normalize
            var slug = !string.IsNullOrWhiteSpace(request.Slug)
                ? NormalizeSlug(request.Slug)
                : GenerateSlugFromTitle(request.Title);

            // aynı slug başka posta mı ait?
            var slugInUse = await _db.Posts
                .AnyAsync(p => p.Id != request.Id && p.Slug == slug && !p.IsDeleted, ct);
            if (slugInUse)
                throw new InvalidOperationException("Bu slug başka bir post tarafından kullanılıyor.");

            // domain davranışıyla güncelle
            post.Update(
                title: request.Title,
                slug: slug,
                content: request.Content,
                categoryId: request.CategoryId,
                summary: request.Summary,
                isActive: request.IsActive
            );

            try
            {
                await _db.SaveChangesAsync(ct);
            }
            catch (PostgresException pex) when (pex.SqlState == "23505")
            {
                _logger.LogWarning(pex, "Post güncellenirken unique ihlali.");
                throw new InvalidOperationException("Bu başlık veya slug zaten kullanılıyor.");
            }

            return new PostSavedDto
            {
                Id = post.Id,
                Title = post.Title,
                Slug = post.Slug,
                CategoryId = post.CategoryId,
                IsPublished = post.IsPublished,
                PublishedAt = post.PublishedAt
            };
        }

        // ------------------------------------------------------------
        // PUBLISH
        // ------------------------------------------------------------
        public async Task<PostSavedDto> PublishAsync(Guid postId, Guid actorUserId, CancellationToken ct)
        {
            var post = await _db.Posts.FirstOrDefaultAsync(p => p.Id == postId && !p.IsDeleted, ct);
            if (post is null)
                throw new InvalidOperationException("Yayınlanacak post bulunamadı.");

            post.Publish(); // domain'de var :contentReference[oaicite:8]{index=8}

            await _db.SaveChangesAsync(ct);

            return new PostSavedDto
            {
                Id = post.Id,
                Title = post.Title,
                Slug = post.Slug,
                CategoryId = post.CategoryId,
                IsPublished = post.IsPublished,
                PublishedAt = post.PublishedAt
            };
        }

        // ------------------------------------------------------------
        // UNPUBLISH
        // ------------------------------------------------------------
        public async Task<PostSavedDto> UnpublishAsync(Guid postId, Guid actorUserId, CancellationToken ct)
        {
            var post = await _db.Posts.FirstOrDefaultAsync(p => p.Id == postId && !p.IsDeleted, ct);
            if (post is null)
                throw new InvalidOperationException("Yayından kaldırılacak post bulunamadı.");

            post.Unpublish(); // domain'de var :contentReference[oaicite:9]{index=9}

            await _db.SaveChangesAsync(ct);

            return new PostSavedDto
            {
                Id = post.Id,
                Title = post.Title,
                Slug = post.Slug,
                CategoryId = post.CategoryId,
                IsPublished = post.IsPublished,
                PublishedAt = post.PublishedAt
            };
        }

        // ------------------------------------------------------------
        // DELETE (soft)
        // ------------------------------------------------------------
        public async Task DeleteAsync(Guid postId, Guid actorUserId, CancellationToken ct)
        {
            var post = await _db.Posts.FirstOrDefaultAsync(p => p.Id == postId && !p.IsDeleted, ct);
            if (post is null)
                return; // idempotent

            post.Delete(); // domain'de var :contentReference[oaicite:10]{index=10}

            await _db.SaveChangesAsync(ct);
        }

        // ------------------------------------------------------------
        // HELPERS
        // ------------------------------------------------------------
        private static string GenerateSlugFromTitle(string title)
        {
            title = (title ?? string.Empty).Trim();
            if (title.Length == 0)
                return Guid.NewGuid().ToString("n")[..8];

            // küçük harfe çevir
            var lower = title.ToLowerInvariant();
            var chars = lower.ToCharArray();

            for (int i = 0; i < chars.Length; i++)
            {
                var ch = chars[i];
                if ((ch >= 'a' && ch <= 'z') || (ch >= '0' && ch <= '9'))
                    continue;

                chars[i] = '-';
            }

            var slug = new string(chars);

            while (slug.Contains("--"))
                slug = slug.Replace("--", "-");

            slug = slug.Trim('-');

            if (string.IsNullOrWhiteSpace(slug))
                slug = Guid.NewGuid().ToString("n")[..8];

            return slug;
        }

        private static string NormalizeSlug(string slug)
        {
            slug = (slug ?? string.Empty).Trim().ToLowerInvariant();
            if (slug.Length == 0)
                return Guid.NewGuid().ToString("n")[..8];

            var chars = slug.ToCharArray();
            for (int i = 0; i < chars.Length; i++)
            {
                var ch = chars[i];
                if ((ch >= 'a' && ch <= 'z') || (ch >= '0' && ch <= '9') || ch == '-')
                    continue;
                chars[i] = '-';
            }

            var result = new string(chars);

            while (result.Contains("--"))
                result = result.Replace("--", "-");

            result = result.Trim('-');

            return string.IsNullOrWhiteSpace(result)
                ? Guid.NewGuid().ToString("n")[..8]
                : result;
        }
    }
}
