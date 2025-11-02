using System;
using System.Collections.Generic;
using System.Linq;

namespace Obeysoft.Domain.Categories
{
    /// <summary>
    /// Blog kategorisi — EF bağımsız Domain entity.
    /// Parent-Child hiyerarşisi ParentId ile kurulur. Children navigation sadece okunur.
    /// </summary>
    public sealed class Category
    {
        // Kimlik ve temel alanlar
        public Guid Id { get; private set; }
        public string Name { get; private set; } = default!;
        public string Slug { get; private set; } = default!;
        public string? Description { get; private set; }
        public bool IsActive { get; private set; }
        public int DisplayOrder { get; private set; }

        // Hiyerarşi
        public Guid? ParentId { get; private set; }

        // Audit
        public DateTimeOffset CreatedAt { get; private set; }
        public DateTimeOffset? UpdatedAt { get; private set; }

        // Navigation (EF için)
        private readonly List<Category> _children = new();
        public IReadOnlyCollection<Category> Children => _children.AsReadOnly();

        // EF/serileştirme için
        private Category() { }

        private Category(Guid id, string name, string slug, string? description, bool isActive, int displayOrder, Guid? parentId)
        {
            Id = id == Guid.Empty ? Guid.NewGuid() : id;

            SetName(name);
            SetSlug(slug);
            SetDescription(description);

            IsActive = isActive;
            SetDisplayOrder(displayOrder);

            ParentId = parentId;

            CreatedAt = DateTimeOffset.UtcNow;
        }

        // -------- FACTORIES --------
        public static Category CreateRoot(string name, string slug, string? description = null, int displayOrder = 0, bool isActive = true)
            => new Category(Guid.NewGuid(), name, slug, description, isActive, displayOrder, null);

        public static Category CreateChild(Guid parentId, string name, string slug, string? description = null, int displayOrder = 0, bool isActive = true)
        {
            if (parentId == Guid.Empty) throw new ArgumentException("Geçerli bir parentId gereklidir.", nameof(parentId));
            return new Category(Guid.NewGuid(), name, slug, description, isActive, displayOrder, parentId);
        }

        // -------- BEHAVIOUR --------
        public void Update(string name, string slug, string? description, bool isActive, int displayOrder, Guid? parentId)
        {
            SetName(name);
            SetSlug(slug);
            SetDescription(description);
            IsActive = isActive;
            SetDisplayOrder(displayOrder);
            ParentId = parentId;
            Touch();
        }

        public void Activate()
        {
            if (!IsActive)
            {
                IsActive = true;
                Touch();
            }
        }

        public void Deactivate()
        {
            if (IsActive)
            {
                IsActive = false;
                Touch();
            }
        }

        public void MoveTo(Guid? parentId)
        {
            if (parentId == Id) throw new InvalidOperationException("Kategori kendisinin altına taşınamaz.");
            ParentId = parentId;
            Touch();
        }

        // -------- VALIDATION / SETTERS --------
        private void SetName(string name)
        {
            name = (name ?? string.Empty).Trim();
            if (name.Length < 2) throw new ArgumentException("Kategori adı en az 2 karakter olmalıdır.", nameof(name));
            if (name.Length > 160) throw new ArgumentException("Kategori adı 160 karakteri aşamaz.", nameof(name));
            Name = name;
        }

        private void SetSlug(string slug)
        {
            slug = (slug ?? string.Empty).Trim().ToLowerInvariant();
            if (slug.Length < 2) throw new ArgumentException("Slug en az 2 karakter olmalıdır.", nameof(slug));
            if (slug.Length > 180) throw new ArgumentException("Slug 180 karakteri aşamaz.", nameof(slug));
            if (!slug.All(ch => (ch >= 'a' && ch <= 'z') || (ch >= '0' && ch <= '9') || ch == '-'))
                throw new ArgumentException("Slug sadece küçük harf, rakam ve tire içerebilir.", nameof(slug));
            Slug = TrimHyphens(slug);
            if (string.IsNullOrWhiteSpace(Slug)) throw new ArgumentException("Geçerli bir slug gereklidir.", nameof(slug));
        }

        private void SetDescription(string? description)
        {
            var v = description?.Trim();
            if (!string.IsNullOrEmpty(v) && v.Length > 1000)
                throw new ArgumentException("Açıklama 1000 karakteri aşamaz.", nameof(description));
            Description = string.IsNullOrWhiteSpace(v) ? null : v;
        }

        private void SetDisplayOrder(int displayOrder)
        {
            if (displayOrder < 0) throw new ArgumentException("DisplayOrder negatif olamaz.", nameof(displayOrder));
            DisplayOrder = displayOrder;
        }

        private static string TrimHyphens(string input)
        {
            int start = 0, end = input.Length - 1;
            while (start <= end && input[start] == '-') start++;
            while (end >= start && input[end] == '-') end--;
            return start > end ? string.Empty : input.Substring(start, end - start + 1);
        }

        private void Touch() => UpdatedAt = DateTimeOffset.UtcNow;
    }
}
