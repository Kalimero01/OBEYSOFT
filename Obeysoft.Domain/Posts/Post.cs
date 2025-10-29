using System;
using System.Linq;

namespace Obeysoft.Domain.Posts
{
    /// <summary>
    /// Blog yazısı (Post) — EF bağımlılığı yok; Domain katmanı.
    /// Kategori ile ilişki CategoryId üzerinden kurulur (Category entity'si Domain'dedir).
    /// </summary>
    public sealed class Post
    {
        // Kimlik ve temel bilgiler
        public Guid Id { get; private set; }
        public string Title { get; private set; } = default!;
        public string Slug { get; private set; } = default!;
        public string? Summary { get; private set; }
        public string Content { get; private set; } = default!;

        // İlişkiler
        public Guid CategoryId { get; private set; }

        // Yayın & durum
        public bool IsPublished { get; private set; }
        public bool IsActive { get; private set; }
        public bool IsDeleted { get; private set; }
        public DateTimeOffset? PublishedAt { get; private set; }

        // Audit
        public DateTimeOffset CreatedAt { get; private set; }
        public DateTimeOffset? UpdatedAt { get; private set; }

        // EF/serileştirme için
        private Post() { }

        private Post(Guid id, string title, string slug, string content, Guid categoryId, string? summary, bool isActive)
        {
            Id = id == Guid.Empty ? Guid.NewGuid() : id;

            SetTitle(title);
            SetSlug(slug);
            SetContent(content);

            Summary = NormalizeNullable(summary);
            SetCategory(categoryId);

            IsActive = isActive;
            IsPublished = false;
            IsDeleted = false;

            CreatedAt = DateTimeOffset.UtcNow;
        }

        // -------- FACTORY --------
        public static Post CreateDraft(string title, string slug, string content, Guid categoryId, string? summary = null, bool isActive = true)
            => new Post(Guid.NewGuid(), title, slug, content, categoryId, summary, isActive);

        // -------- BEHAVIOUR --------
        public void Update(string title, string slug, string content, Guid categoryId, string? summary, bool isActive)
        {
            SetTitle(title);
            SetSlug(slug);
            SetContent(content);
            Summary = NormalizeNullable(summary);
            SetCategory(categoryId);
            IsActive = isActive;
            Touch();
        }

        public void MoveToCategory(Guid categoryId)
        {
            SetCategory(categoryId);
            Touch();
        }

        public void Publish()
        {
            if (!IsPublished)
            {
                IsPublished = true;
                PublishedAt = DateTimeOffset.UtcNow;
                Touch();
            }
        }

        public void Unpublish()
        {
            if (IsPublished)
            {
                IsPublished = false;
                PublishedAt = null;
                Touch();
            }
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

        public void Delete()
        {
            if (!IsDeleted)
            {
                IsDeleted = true;
                Touch();
            }
        }

        public void Restore()
        {
            if (IsDeleted)
            {
                IsDeleted = false;
                Touch();
            }
        }

        // -------- VALIDATION / SETTERS --------
        private void SetTitle(string title)
        {
            title = (title ?? string.Empty).Trim();
            if (title.Length < 3) throw new ArgumentException("Başlık en az 3 karakter olmalıdır.", nameof(title));
            if (title.Length > 180) throw new ArgumentException("Başlık 180 karakteri aşamaz.", nameof(title));
            Title = title;
        }

        private void SetSlug(string slug)
        {
            slug = (slug ?? string.Empty).Trim().ToLowerInvariant();

            if (slug.Length < 2) throw new ArgumentException("Slug en az 2 karakter olmalıdır.", nameof(slug));
            if (slug.Length > 200) throw new ArgumentException("Slug 200 karakteri aşamaz.", nameof(slug));
            if (!slug.All(ch => (ch >= 'a' && ch <= 'z') || (ch >= '0' && ch <= '9') || ch == '-'))
                throw new ArgumentException("Slug sadece küçük harf, rakam ve tire içerebilir.", nameof(slug));

            slug = TrimHyphens(slug);
            while (slug.Contains("--")) slug = slug.Replace("--", "-");

            if (string.IsNullOrWhiteSpace(slug)) throw new ArgumentException("Geçerli bir slug gereklidir.", nameof(slug));
            Slug = slug;
        }

        private void SetContent(string content)
        {
            content = (content ?? string.Empty).Trim();
            if (content.Length < 10) throw new ArgumentException("İçerik en az 10 karakter olmalıdır.", nameof(content));
            Content = content;
        }

        private void SetCategory(Guid categoryId)
        {
            if (categoryId == Guid.Empty) throw new ArgumentException("Geçerli bir CategoryId gereklidir.", nameof(categoryId));
            CategoryId = categoryId;
        }

        private static string? NormalizeNullable(string? value)
        {
            var v = value?.Trim();
            return string.IsNullOrWhiteSpace(v) ? null : v;
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
