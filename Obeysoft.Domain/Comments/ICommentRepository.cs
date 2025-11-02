using System;
using System.Threading;
using System.Threading.Tasks;

namespace Obeysoft.Domain.Comments
{
    /// <summary>
    /// Yorumlara yönelik kalıcılaştırma sözleşmesi (DDD: Domain → Infra implement eder).
    /// </summary>
    public interface ICommentRepository
    {
        /// <summary>Post var mı?</summary>
        Task<bool> PostExistsAsync(Guid postId, CancellationToken ct);

        /// <summary>Parent yorum var mı? (threading için)</summary>
        Task<bool> CommentExistsAsync(Guid commentId, CancellationToken ct);

        /// <summary>Yorum ekle (henüz commit yapmadan tracking’e alır).</summary>
        Task AddAsync(Comment comment, CancellationToken ct);

        /// <summary>Değişiklikleri kalıcılaştır.</summary>
        Task SaveChangesAsync(CancellationToken ct);
    }
}
