// FILE: Obeysoft.Infrastructure/Comments/CommentRepository.cs
using Microsoft.EntityFrameworkCore;
using Obeysoft.Domain.Comments;
using Obeysoft.Infrastructure.Persistence;

namespace Obeysoft.Infrastructure.Comments
{
    /// <summary>
    /// Yorumlar için EF Core repository implementasyonu.
    /// </summary>
    internal sealed class CommentRepository : ICommentRepository
    {
        private readonly BlogDbContext _db;

        public CommentRepository(BlogDbContext db) => _db = db;

        public async Task AddAsync(Comment comment, CancellationToken ct)
        {
            await _db.Comments.AddAsync(comment, ct);
        }

        public Task<bool> PostExistsAsync(Guid postId, CancellationToken ct)
        {
            return _db.Posts.AnyAsync(p => p.Id == postId, ct);
        }

        public Task<bool> CommentExistsAsync(Guid commentId, CancellationToken ct)
        {
            return _db.Comments.AnyAsync(c => c.Id == commentId, ct);
        }

        public Task SaveChangesAsync(CancellationToken ct)
        {
            return _db.SaveChangesAsync(ct);
        }
    }
}