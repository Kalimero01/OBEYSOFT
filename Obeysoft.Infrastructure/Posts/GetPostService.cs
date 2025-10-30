using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Obeysoft.Application.Posts;
using Obeysoft.Infrastructure.Persistence;

namespace Obeysoft.Infrastructure.Posts
{
    /// <summary>
    /// Post okuma servisinin EF Core implementasyonu.
    /// Yalnızca yayınlanmış ve aktif içerikleri döndürür (soft delete filtreleri DbContext'te tanımlı).
    /// </summary>
    public sealed class GetPostService : IGetPostService
    {
        private readonly BlogDbContext _db;

        public GetPostService(BlogDbContext db) => _db = db;

        public async Task<PagedResult<GetPostListItemDto>> GetPublishedAsync(
            int page,
            int pageSize,
            string? categorySlug,
            string? search,
            CancellationToken ct)
        {
            page = page <= 0 ? 1 : page;
            pageSize = pageSize <= 0 ? 10 : Math.Min(pageSize, 100);

            var q =
                from p in _db.Posts.AsNoTracking()
                join c in _db.Categories.AsNoTracking() on p.CategoryId equals c.Id
                where p.IsActive && p.IsPublished && c.IsActive
                select new
                {
                    p.Id,
                    p.Title,
                    p.Slug,
                    p.Summary,
                    p.PublishedAt,
                    p.IsPublished,
                    CategoryName = c.Name,
                    CategorySlug = c.Slug
                };

            if (!string.IsNullOrWhiteSpace(categorySlug))
            {
                var slug = categorySlug.Trim().ToLowerInvariant();
                q = q.Where(x => x.CategorySlug == slug);
            }

            if (!string.IsNullOrWhiteSpace(search))
            {
                var term = search.Trim();
                // Basit arama: başlık/özet
                q = q.Where(x => EF.Functions.ILike(x.Title, $"%{term}%") ||
                                 (x.Summary != null && EF.Functions.ILike(x.Summary, $"%{term}%")));
            }

            // En yeni yayımlananlar önce
            q = q.OrderByDescending(x => x.PublishedAt).ThenByDescending(x => x.Id);

            var total = await q.CountAsync(ct);

            var items = await q
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(x => new GetPostListItemDto
                {
                    Id = x.Id,
                    Title = x.Title,
                    Slug = x.Slug,
                    Summary = x.Summary,
                    CategoryName = x.CategoryName,
                    CategorySlug = x.CategorySlug,
                    PublishedAt = x.PublishedAt,
                    IsPublished = x.IsPublished
                })
                .ToListAsync(ct);

            return new PagedResult<GetPostListItemDto>(items, page, pageSize, total);
        }

        public async Task<GetPostDetailDto?> GetBySlugAsync(string slug, CancellationToken ct)
        {
            slug = (slug ?? string.Empty).Trim().ToLowerInvariant();
            if (slug.Length == 0) return null;

            var q =
                from p in _db.Posts.AsNoTracking()
                join c in _db.Categories.AsNoTracking() on p.CategoryId equals c.Id
                where p.IsActive && c.IsActive && p.Slug == slug
                select new GetPostDetailDto
                {
                    Id = p.Id,
                    Title = p.Title,
                    Slug = p.Slug,
                    Summary = p.Summary,
                    Content = p.Content,
                    CategoryId = c.Id,
                    CategoryName = c.Name,
                    CategorySlug = c.Slug,
                    PublishedAt = p.PublishedAt,
                    IsPublished = p.IsPublished,
                    IsActive = p.IsActive
                };

            // Yayında olmayanları burada göstermiyoruz (kamu uçları).
            q = q.Where(x => x.IsPublished);

            return await q.FirstOrDefaultAsync(ct);
        }
    }
}