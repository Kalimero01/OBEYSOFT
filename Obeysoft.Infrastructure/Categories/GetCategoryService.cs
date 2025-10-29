using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Obeysoft.Application.Categories;
using Obeysoft.Infrastructure.Persistence;
using Obeysoft.Domain.Categories;

namespace Obeysoft.Infrastructure.Categories
{
    /// <summary>
    /// Kategori okuma servisinin EF Core implementasyonu.
    /// - Sol menü için aktif kategori listesi
    /// - Hiyerarşik ağaç (aktifler)
    /// - Çocukları getirme
    /// - Slug ve arama
    /// </summary>
    public sealed class GetCategoryService : IGetCategoryService
    {
        private readonly BlogDbContext _db;

        public GetCategoryService(BlogDbContext db) => _db = db;

        public async Task<IReadOnlyList<GetCategoryListItemDto>> GetAllActiveAsync(CancellationToken ct)
        {
            var q = _db.Categories
                .AsNoTracking()
                .Where(c => c.IsActive)
                .OrderBy(c => c.DisplayOrder)
                .ThenBy(c => c.Name);

            var list = await q
                .Select(c => new GetCategoryListItemDto
                {
                    Id = c.Id,
                    Name = c.Name,
                    Slug = c.Slug,
                    Description = c.Description,
                    IsActive = c.IsActive,
                    DisplayOrder = c.DisplayOrder,
                    ParentId = c.ParentId
                })
                .ToListAsync(ct);

            return list;
        }

        public async Task<IReadOnlyList<CategoryTreeNodeDto>> GetActiveTreeAsync(CancellationToken ct)
        {
            // Tüm aktif kategorileri çekip bellekte ağaç kuruyoruz (tek roundtrip).
            var categories = await _db.Categories
                .AsNoTracking()
                .Where(c => c.IsActive)
                .OrderBy(c => c.ParentId)
                .ThenBy(c => c.DisplayOrder)
                .ThenBy(c => c.Name)
                .Select(c => new
                {
                    c.Id,
                    c.Name,
                    c.Slug,
                    c.DisplayOrder,
                    c.IsActive,
                    c.ParentId
                })
                .ToListAsync(ct);

            // Id -> Node sözlüğü
            var dict = categories.ToDictionary(
                c => c.Id,
                c => new CategoryTreeNodeDto
                {
                    Id = c.Id,
                    Name = c.Name,
                    Slug = c.Slug,
                    DisplayOrder = c.DisplayOrder,
                    IsActive = c.IsActive,
                    Children = Array.Empty<CategoryTreeNodeDto>()
                });

            // ParentId -> children list
            var childrenMap = new Dictionary<Guid, List<CategoryTreeNodeDto>>();

            foreach (var c in categories)
            {
                if (c.ParentId is Guid pid && dict.TryGetValue(c.Id, out var node))
                {
                    if (!childrenMap.TryGetValue(pid, out var list))
                    {
                        list = new List<CategoryTreeNodeDto>();
                        childrenMap[pid] = list;
                    }
                    list.Add(node);
                }
            }

            // Çocukları sırala ve ata
            foreach (var kv in childrenMap)
            {
                var ordered = kv.Value
                    .OrderBy(x => x.DisplayOrder)
                    .ThenBy(x => x.Name)
                    .ToArray();

                // dict’te parent node varsa Children güncelle
                if (dict.TryGetValue(kv.Key, out var parent))
                {
                    // parent'ı yeni bir kopya ile güncellemek yerine,
                    // mutability yok; bu yüzden yeni node oluşturacağız.
                    // Ama DTO immutable init-only, Children set edilebilsin diye aşağıda helper kullanıyoruz.
                    ReplaceChildren(dict, parent.Id, ordered);
                }
            }

            // Kökler: ParentId == null
            var roots = categories
                .Where(c => c.ParentId == null)
                .Select(c => dict[c.Id])
                .OrderBy(x => x.DisplayOrder)
                .ThenBy(x => x.Name)
                .ToArray();

            return roots;

            // Local function - dict içindeki node’un children’ını değiştir
            static void ReplaceChildren(Dictionary<Guid, CategoryTreeNodeDto> d, Guid id, IReadOnlyList<CategoryTreeNodeDto> kids)
            {
                var old = d[id];
                d[id] = new CategoryTreeNodeDto
                {
                    Id = old.Id,
                    Name = old.Name,
                    Slug = old.Slug,
                    DisplayOrder = old.DisplayOrder,
                    IsActive = old.IsActive,
                    Children = kids
                };
            }
        }

        public async Task<IReadOnlyList<GetCategoryListItemDto>> GetChildrenAsync(Guid parentId, CancellationToken ct)
        {
            if (parentId == Guid.Empty) return Array.Empty<GetCategoryListItemDto>();

            var q = _db.Categories
                .AsNoTracking()
                .Where(c => c.IsActive && c.ParentId == parentId)
                .OrderBy(c => c.DisplayOrder)
                .ThenBy(c => c.Name);

            var list = await q
                .Select(c => new GetCategoryListItemDto
                {
                    Id = c.Id,
                    Name = c.Name,
                    Slug = c.Slug,
                    Description = c.Description,
                    IsActive = c.IsActive,
                    DisplayOrder = c.DisplayOrder,
                    ParentId = c.ParentId
                })
                .ToListAsync(ct);

            return list;
        }

        public async Task<GetCategoryListItemDto?> GetBySlugAsync(string slug, CancellationToken ct)
        {
            slug = (slug ?? string.Empty).Trim().ToLowerInvariant();
            if (slug.Length == 0) return null;

            var c = await _db.Categories
                .AsNoTracking()
                .Where(x => x.Slug == slug)
                .Select(x => new GetCategoryListItemDto
                {
                    Id = x.Id,
                    Name = x.Name,
                    Slug = x.Slug,
                    Description = x.Description,
                    IsActive = x.IsActive,
                    DisplayOrder = x.DisplayOrder,
                    ParentId = x.ParentId
                })
                .FirstOrDefaultAsync(ct);

            return c;
        }

        public async Task<IReadOnlyList<GetCategoryListItemDto>> SearchAsync(string query, CancellationToken ct)
        {
            query = (query ?? string.Empty).Trim();
            if (query.Length == 0) return Array.Empty<GetCategoryListItemDto>();

            var q = _db.Categories
                .AsNoTracking()
                .Where(c => c.IsActive &&
                            (EF.Functions.ILike(c.Name, $"%{query}%") ||
                             EF.Functions.ILike(c.Slug, $"%{query}%")))
                .OrderBy(c => c.DisplayOrder)
                .ThenBy(c => c.Name);

            var list = await q
                .Select(c => new GetCategoryListItemDto
                {
                    Id = c.Id,
                    Name = c.Name,
                    Slug = c.Slug,
                    Description = c.Description,
                    IsActive = c.IsActive,
                    DisplayOrder = c.DisplayOrder,
                    ParentId = c.ParentId
                })
                .ToListAsync(ct);

            return list;
        }
    }
}
