using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Obeysoft.Infrastructure.Persistence;

namespace Obeysoft.Api.Controllers
{
    [ApiController]
    [Route("api/Admin/[controller]")]
    [Authorize(Roles = "Admin")]
    public sealed class AdminUsersController : ControllerBase
    {
        private readonly BlogDbContext _db;
        public AdminUsersController(BlogDbContext db) => _db = db;

        [HttpGet]
        public async Task<IActionResult> List(CancellationToken ct)
        {
            var list = await _db.Users.AsNoTracking()
                .OrderBy(u => u.DisplayName)
                .Select(u => new { u.Id, u.DisplayName, u.Email, Role = u.Role.ToString(), u.IsActive })
                .ToListAsync(ct);
            return Ok(list);
        }

        [HttpPost("{id:guid}/activate")]
        public async Task<IActionResult> Activate(Guid id, CancellationToken ct)
        {
            var u = await _db.Users.FirstOrDefaultAsync(x => x.Id == id, ct);
            if (u is null) return NotFound();
            u.Activate();
            await _db.SaveChangesAsync(ct);
            return Ok();
        }

        [HttpPost("{id:guid}/deactivate")]
        public async Task<IActionResult> Deactivate(Guid id, CancellationToken ct)
        {
            var u = await _db.Users.FirstOrDefaultAsync(x => x.Id == id, ct);
            if (u is null) return NotFound();
            u.Deactivate();
            await _db.SaveChangesAsync(ct);
            return Ok();
        }
    }
}


