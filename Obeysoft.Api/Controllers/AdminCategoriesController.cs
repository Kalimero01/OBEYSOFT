using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Obeysoft.Infrastructure.Persistence;
using Obeysoft.Domain.Categories;

namespace Obeysoft.Api.Controllers
{
    [ApiController]
    [Route("api/Admin/[controller]")]
    [Authorize(Roles = "Admin")]
    public sealed class AdminCategoriesController : ControllerBase
    {
        private readonly BlogDbContext _db;
        public AdminCategoriesController(BlogDbContext db) => _db = db;

        public sealed record UpsertCategoryDto(string Name, string Slug, string? Description, Guid? ParentId, int DisplayOrder, bool IsActive);

        [HttpGet]
        public async Task<IActionResult> List(CancellationToken ct)
        {
            var rows = await _db.Categories
                .AsNoTracking()
                .OrderBy(c => c.DisplayOrder)
                .ThenBy(c => c.Name)
                .Select(c => new
                {
                    c.Id,
                    c.Name,
                    c.Slug,
                    c.Description,
                    c.ParentId,
                    c.DisplayOrder,
                    c.IsActive,
                    ParentName = _db.Categories.Where(x => x.Id == c.ParentId).Select(x => x.Name).FirstOrDefault()
                })
                .ToListAsync(ct);

            return Ok(rows);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] UpsertCategoryDto dto, CancellationToken ct)
        {
            var entity = dto.ParentId is null
                ? Category.CreateRoot(dto.Name, dto.Slug, dto.Description, dto.DisplayOrder, dto.IsActive)
                : Category.CreateChild(dto.ParentId.Value, dto.Name, dto.Slug, dto.Description, dto.DisplayOrder, dto.IsActive);

            _db.Categories.Add(entity);
            await _db.SaveChangesAsync(ct);
            return Ok(new { id = entity.Id });
        }

        [HttpPut("{id:guid}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpsertCategoryDto dto, CancellationToken ct)
        {
            var c = await _db.Categories.FirstOrDefaultAsync(x => x.Id == id, ct);
            if (c is null) return NotFound();

            c.Update(dto.Name, dto.Slug, dto.Description, dto.IsActive, dto.DisplayOrder, dto.ParentId);
            await _db.SaveChangesAsync(ct);
            return Ok(new { id = c.Id });
        }

        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
        {
            var c = await _db.Categories.FirstOrDefaultAsync(x => x.Id == id, ct);
            if (c is null) return NotFound();
            _db.Categories.Remove(c);
            await _db.SaveChangesAsync(ct);
            return NoContent();
        }
    }
}


