using System;

namespace Obeysoft.Domain.Comments
{
    /// <summary>
    /// Yorum: yalnızca giriş yapmış kullanıcı oluşturabilir (AuthorId gerekir).
    /// Post ile ilişki PostId üzerinden kurulur. Threaded yapı için ParentId opsiyoneldir.
    /// EF bağımlılığı yoktur; tamamen Domain katmanıdır.
    /// </summary>
    public sealed class Comment
    {
        // Kimlik
        public Guid Id { get; private set; }

        // İlişkiler
        public Guid PostId { get; private set; }
        public Guid AuthorId { get; private set; } // User.Id (GUID)

        // Threading (yanıt)
        public Guid? ParentId { get; private set; }

        // İçerik
        public string Content { get; private set; } = default!;

        // Durumlar
        public bool IsActive { get; private set; }
        public bool IsDeleted { get; private set; }
        public bool IsApproved { get; private set; } // Moderasyon için (Admin onayı)

        // Zaman damgaları
        public DateTimeOffset CreatedAt { get; private set; }
        public DateTimeOffset? UpdatedAt { get; private set; }

        // EF/serileştirme için
        private Comment() { }

        private Comment(Guid id, Guid postId, Guid authorId, string content, Guid? parentId, bool isActive, bool isApproved)
        {
            Id = id == Guid.Empty ? Guid.NewGuid() : id;

            SetPost(postId);
            SetAuthor(authorId);
            SetContent(content);

            ParentId = parentId;

            IsActive = isActive;
            IsApproved = isApproved;
            IsDeleted = false;

            CreatedAt = DateTimeOffset.UtcNow;
        }

        // -------- FACTORY --------
        /// <summary> Varsayılan: aktif, onay bekliyor (IsApproved=false). </summary>
        public static Comment Create(Guid postId, Guid authorId, string content, Guid? parentId = null, bool isActive = true)
            => new Comment(Guid.NewGuid(), postId, authorId, content, parentId, isActive, isApproved: false);

        // -------- BEHAVIOUR --------
        public void UpdateContent(string content)
        {
            SetContent(content);
            Touch();
        }

        public void MoveToParent(Guid? parentId)
        {
            if (parentId.HasValue && parentId.Value == Id)
                throw new InvalidOperationException("Yorum kendisinin altına taşınamaz.");
            ParentId = parentId;
            Touch();
        }

        public void AttachToPost(Guid postId)
        {
            SetPost(postId);
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

        public void Approve()
        {
            if (!IsApproved)
            {
                IsApproved = true;
                Touch();
            }
        }

        public void Reject()
        {
            if (IsApproved)
            {
                IsApproved = false;
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
        private void SetPost(Guid postId)
        {
            if (postId == Guid.Empty) throw new ArgumentException("Geçerli bir PostId gereklidir.", nameof(postId));
            PostId = postId;
        }

        private void SetAuthor(Guid authorId)
        {
            if (authorId == Guid.Empty) throw new ArgumentException("Geçerli bir AuthorId gereklidir.", nameof(authorId));
            AuthorId = authorId;
        }

        private void SetContent(string content)
        {
            content = (content ?? string.Empty).Trim();
            if (content.Length < 2) throw new ArgumentException("Yorum en az 2 karakter olmalıdır.", nameof(content));
            if (content.Length > 4000) throw new ArgumentException("Yorum 4000 karakteri aşamaz.", nameof(content));
            Content = content;
        }

        private void Touch() => UpdatedAt = DateTimeOffset.UtcNow;
    }
}
