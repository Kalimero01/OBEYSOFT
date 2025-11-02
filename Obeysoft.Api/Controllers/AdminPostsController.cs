using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Obeysoft.Domain.Posts;
using Obeysoft.Infrastructure.Persistence;

namespace Obeysoft.Api.Controllers
{
    [ApiController]
    [Route("api/Admin/[controller]")]
    [Authorize(Roles = "Admin")]
    public sealed class AdminPostsController : ControllerBase
    {
        private readonly BlogDbContext _db;
        public AdminPostsController(BlogDbContext db) => _db = db;

        public sealed record UpsertPostDto(string Title, string Slug, string Content, Guid CategoryId, string? Summary, bool IsActive);

        [HttpGet]
        public async Task<IActionResult> List([FromQuery] int page = 1, [FromQuery] int pageSize = 10, [FromQuery] string? search = null, CancellationToken ct = default)
        {
            page = page <= 0 ? 1 : page;
            pageSize = pageSize <= 0 ? 10 : Math.Min(pageSize, 100);

            var query =
                from p in _db.Posts.AsNoTracking()
                join c in _db.Categories.AsNoTracking() on p.CategoryId equals c.Id
                orderby p.CreatedAt descending
                select new
                {
                    p.Id,
                    p.Title,
                    p.Slug,
                    p.Content,
                    p.IsPublished,
                    p.IsActive,
                    p.CreatedAt,
                    p.PublishedAt,
                    CategoryId = c.Id,
                    CategoryName = c.Name
                };

            if (!string.IsNullOrWhiteSpace(search))
            {
                var term = search.Trim();
                query = query.Where(x => EF.Functions.ILike(x.Title, $"%{term}%"));
            }

            var totalCount = await query.CountAsync(ct);

            var items = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(x => new
                {
                    x.Id,
                    x.Title,
                    x.Slug,
                    x.Content,
                    x.IsPublished,
                    x.IsActive,
                    x.CreatedAt,
                    x.PublishedAt,
                    x.CategoryId,
                    x.CategoryName
                })
                .ToListAsync(ct);

            var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

            return Ok(new
            {
                items,
                page,
                pageSize,
                totalCount,
                totalPages
            });
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] UpsertPostDto dto, CancellationToken ct)
        {
            var p = Post.CreateDraft(dto.Title, dto.Slug, dto.Content, dto.CategoryId, dto.Summary, dto.IsActive);
            _db.Posts.Add(p);
            await _db.SaveChangesAsync(ct);
            return Ok(new { id = p.Id });
        }

        [HttpPut("{id:guid}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpsertPostDto dto, CancellationToken ct)
        {
            var p = await _db.Posts.FirstOrDefaultAsync(x => x.Id == id, ct);
            if (p is null) return NotFound();

            p.Update(dto.Title, dto.Slug, dto.Content, dto.CategoryId, dto.Summary, dto.IsActive);
            await _db.SaveChangesAsync(ct);
            return Ok(new { id = p.Id });
        }

        [HttpPost("{id:guid}/publish")]
        public async Task<IActionResult> Publish(Guid id, CancellationToken ct)
        {
            var p = await _db.Posts.FirstOrDefaultAsync(x => x.Id == id, ct);
            if (p is null) return NotFound();
            p.Publish();
            await _db.SaveChangesAsync(ct);
            return Ok();
        }

        [HttpPost("{id:guid}/unpublish")]
        public async Task<IActionResult> Unpublish(Guid id, CancellationToken ct)
        {
            var p = await _db.Posts.FirstOrDefaultAsync(x => x.Id == id, ct);
            if (p is null) return NotFound();
            p.Unpublish();
            await _db.SaveChangesAsync(ct);
            return Ok();
        }

        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
        {
            var p = await _db.Posts.FirstOrDefaultAsync(x => x.Id == id, ct);
            if (p is null) return NotFound();
            _db.Posts.Remove(p);
            await _db.SaveChangesAsync(ct);
            return NoContent();
        }
    }
}


