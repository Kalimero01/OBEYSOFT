using System;

namespace Obeysoft.Domain.Navigation
{
    /// <summary>
    /// Üst menü/yan menü ögeleri için basit hiyerarşik yapı.
    /// </summary>
    public sealed class NavigationItem
    {
        public Guid Id { get; private set; }
        public string Label { get; private set; } = default!;
        public string Href { get; private set; } = default!;
        public Guid? ParentId { get; private set; }
        public int DisplayOrder { get; private set; }
        public bool IsActive { get; private set; }
        public DateTimeOffset CreatedAt { get; private set; }
        public DateTimeOffset? UpdatedAt { get; private set; }

        private NavigationItem() { }

        private NavigationItem(string label, string href, Guid? parentId, int displayOrder, bool isActive)
        {
            Id = Guid.NewGuid();
            SetLabel(label);
            SetHref(href);
            ParentId = parentId;
            DisplayOrder = displayOrder;
            IsActive = isActive;
            CreatedAt = DateTimeOffset.UtcNow;
        }

        public static NavigationItem Create(string label, string href, Guid? parentId = null, int displayOrder = 0, bool isActive = true)
            => new NavigationItem(label, href, parentId, displayOrder, isActive);

        public void Update(string label, string href, Guid? parentId, int displayOrder, bool isActive)
        {
            SetLabel(label);
            SetHref(href);
            ParentId = parentId;
            DisplayOrder = displayOrder;
            IsActive = isActive;
            UpdatedAt = DateTimeOffset.UtcNow;
        }

        private void SetLabel(string label)
        {
            label = (label ?? string.Empty).Trim();
            if (label.Length < 1 || label.Length > 160) throw new ArgumentException("Geçerli etiket gereklidir.", nameof(label));
            Label = label;
        }

        private void SetHref(string href)
        {
            href = (href ?? string.Empty).Trim();
            if (href.Length < 1 || href.Length > 512) throw new ArgumentException("Geçerli bağlantı gereklidir.", nameof(href));
            Href = href;
        }
    }
}



