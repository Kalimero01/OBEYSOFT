using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Obeysoft.Domain.Navigation;
using Obeysoft.Infrastructure.Persistence;

namespace Obeysoft.Api.Controllers
{
    [ApiController]
    [Route("api/Admin/[controller]")]
    [Authorize(Roles = "Admin")]
    public sealed class NavigationController : ControllerBase
    {
        private readonly BlogDbContext _db;
        public NavigationController(BlogDbContext db) => _db = db;

        public sealed record UpsertDto(string Label, string Href, Guid? ParentId, int DisplayOrder, bool IsActive);

        [HttpGet]
        public async Task<IActionResult> List(CancellationToken ct)
        {
            var list = await _db.NavigationItems.AsNoTracking().OrderBy(x => x.ParentId).ThenBy(x => x.DisplayOrder).ToListAsync(ct);
            return Ok(list);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] UpsertDto dto, CancellationToken ct)
        {
            var n = NavigationItem.Create(dto.Label, dto.Href, dto.ParentId, dto.DisplayOrder, dto.IsActive);
            _db.NavigationItems.Add(n);
            await _db.SaveChangesAsync(ct);
            return Ok(new { id = n.Id });
        }

        [HttpPut("{id:guid}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpsertDto dto, CancellationToken ct)
        {
            var n = await _db.NavigationItems.FirstOrDefaultAsync(x => x.Id == id, ct);
            if (n is null) return NotFound();
            n.Update(dto.Label, dto.Href, dto.ParentId, dto.DisplayOrder, dto.IsActive);
            await _db.SaveChangesAsync(ct);
            return Ok();
        }

        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
        {
            var n = await _db.NavigationItems.FirstOrDefaultAsync(x => x.Id == id, ct);
            if (n is null) return NotFound();
            _db.NavigationItems.Remove(n);
            await _db.SaveChangesAsync(ct);
            return NoContent();
        }
    }
}


