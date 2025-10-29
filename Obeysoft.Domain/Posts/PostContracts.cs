using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Obeysoft.Application.Posts
{
    /// <summary>
    /// Liste sonuçları için ortak sayfalama sarmalayıcısı.
    /// </summary>
    public sealed class PagedResult<T>
    {
        public IReadOnlyList<T> Items { get; }
        public int Page { get; }
        public int PageSize { get; }
        public int TotalCount { get; }
        public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);

        public PagedResult(IReadOnlyList<T> items, int page, int pageSize, int totalCount)
        {
            Items = items ?? Array.Empty<T>();
            Page = page < 1 ? 1 : page;
            PageSize = pageSize < 1 ? 1 : pageSize;
            TotalCount = totalCount < 0 ? 0 : totalCount;
        }
    }

    /// <summary>
    /// Liste ekranı (akış) için hafif öğe.
    /// </summary>
    public sealed class GetPostListItemDto
    {
        public Guid Id { get; init; }
        public string Title { get; init; } = default!;
        public string Slug { get; init; } = default!;
        public string? Summary { get; init; }
        public string CategoryName { get; init; } = default!;
        public string CategorySlug { get; init; } = default!;
        public DateTimeOffset? PublishedAt { get; init; }
        public bool IsPublished { get; init; }
    }

    /// <summary>
    /// Detay ekranı için içerik dahil geniş DTO.
    /// </summary>
    public sealed class GetPostDetailDto
    {
        public Guid Id { get; init; }
        public string Title { get; init; } = default!;
        public string Slug { get; init; } = default!;
        public string? Summary { get; init; }
        public string Content { get; init; } = default!;
        public Guid CategoryId { get; init; }
        public string CategoryName { get; init; } = default!;
        public string CategorySlug { get; init; } = default!;
        public DateTimeOffset? PublishedAt { get; init; }
        public bool IsPublished { get; init; }
        public bool IsActive { get; init; }
    }

    /// <summary>
    /// Post okuma senaryoları için sözleşme (Application katmanı).
    /// Implementasyon Infrastructure içinde yapılacaktır.
    /// </summary>
    public interface IGetPostService
    {
        /// <summary>
        /// Yayında olan postları sayfalı getirir. (Opsiyonel filtreler: kategori slug, arama metni)
        /// </summary>
        Task<PagedResult<GetPostListItemDto>> GetPublishedAsync(
            int page,
            int pageSize,
            string? categorySlug,
            string? search,
            CancellationToken ct);

        /// <summary>
        /// Slug'a göre detay döner (yayında değilse admin yetkisi gerektirecek overload'lar ayrıca eklenebilir).
        /// </summary>
        Task<GetPostDetailDto?> GetBySlugAsync(
            string slug,
            CancellationToken ct);
    }
}
