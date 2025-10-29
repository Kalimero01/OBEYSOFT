using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Obeysoft.Application.Categories
{
    /// <summary>
    /// Sol menü için düz liste öğesi (listeleme/filtreleme).
    /// </summary>
    public sealed class GetCategoryListItemDto
    {
        public Guid Id { get; init; }
        public string Name { get; init; } = default!;
        public string Slug { get; init; } = default!;
        public string? Description { get; init; }
        public bool IsActive { get; init; }
        public int DisplayOrder { get; init; }
        public Guid? ParentId { get; init; }
    }

    /// <summary>
    /// Hiyerarşik ağaç görünümü için DTO (çok seviyeli children).
    /// </summary>
    public sealed class CategoryTreeNodeDto
    {
        public Guid Id { get; init; }
        public string Name { get; init; } = default!;
        public string Slug { get; init; } = default!;
        public int DisplayOrder { get; init; }
        public bool IsActive { get; init; }
        public IReadOnlyList<CategoryTreeNodeDto> Children { get; init; } = Array.Empty<CategoryTreeNodeDto>();
    }

    /// <summary>
    /// Kategori okuma senaryoları (Application sözleşmeleri).
    /// Implementasyon Infrastructure içinde yapılacaktır.
    /// </summary>
    public interface IGetCategoryService
    {
        /// <summary>
        /// Tüm aktif kategorileri düz liste döndürür (menü oluşturma veya seçim listeleri için).
        /// </summary>
        Task<IReadOnlyList<GetCategoryListItemDto>> GetAllActiveAsync(CancellationToken ct);

        /// <summary>
        /// Kökten başlayan tam ağaç yapısını (yalnızca aktifler) döndürür.
        /// </summary>
        Task<IReadOnlyList<CategoryTreeNodeDto>> GetActiveTreeAsync(CancellationToken ct);

        /// <summary>
        /// Bir kategorinin doğrudan çocuklarını (aktif) döndürür.
        /// </summary>
        Task<IReadOnlyList<GetCategoryListItemDto>> GetChildrenAsync(Guid parentId, CancellationToken ct);

        /// <summary>
        /// Slug'a göre kategori getirir (aktif/pasif fark etmeksizin).
        /// </summary>
        Task<GetCategoryListItemDto?> GetBySlugAsync(string slug, CancellationToken ct);

        /// <summary>
        /// İsim/slug ile arama (aktifler üzerinde).
        /// </summary>
        Task<IReadOnlyList<GetCategoryListItemDto>> SearchAsync(string query, CancellationToken ct);
    }
}
