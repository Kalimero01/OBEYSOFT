using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Obeysoft.Infrastructure.Persistence;

namespace Obeysoft.Api.Controllers
{
    [ApiController]
    [Route("api/Admin/[controller]")]
    [Authorize(Roles = "Admin")]
    public sealed class AdminCommentsController : ControllerBase
    {
        private readonly BlogDbContext _db;
        public AdminCommentsController(BlogDbContext db) => _db = db;

        [HttpGet]
        public async Task<IActionResult> List(CancellationToken ct)
        {
            var rows = await _db.Comments.AsNoTracking()
                .OrderByDescending(c => c.CreatedAt)
                .Select(c => new
                {
                    c.Id,
                    PostTitle = _db.Posts.Where(p => p.Id == c.PostId).Select(p => p.Title).FirstOrDefault(),
                    Author = _db.Users.Where(u => u.Id == c.AuthorId).Select(u => u.DisplayName).FirstOrDefault() ?? "",
                    Content = c.Content,
                    IsApproved = c.IsApproved,
                    CreatedAt = c.CreatedAt
                })
                .ToListAsync(ct);
            return Ok(rows);
        }

        [HttpPost("{id:guid}/approve")]
        public async Task<IActionResult> Approve(Guid id, CancellationToken ct)
        {
            var c = await _db.Comments.FirstOrDefaultAsync(x => x.Id == id, ct);
            if (c is null) return NotFound();
            c.Approve();
            await _db.SaveChangesAsync(ct);
            return Ok();
        }

        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
        {
            var c = await _db.Comments.FirstOrDefaultAsync(x => x.Id == id, ct);
            if (c is null) return NotFound();
            _db.Comments.Remove(c);
            await _db.SaveChangesAsync(ct);
            return NoContent();
        }
    }
}


