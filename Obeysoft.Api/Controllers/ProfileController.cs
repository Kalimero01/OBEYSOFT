using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Obeysoft.Infrastructure.Persistence;
using System.Security.Claims;

namespace Obeysoft.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public sealed class ProfileController : ControllerBase
    {
        private readonly BlogDbContext _db;

        public ProfileController(BlogDbContext db) => _db = db;

        public sealed record ProfileDto(Guid UserId, int? Age, string? Gender, string? City, string? AvatarUrl);
        public sealed record UpdateProfileRequest(int? Age, string? Gender, string? City, string? AvatarUrl);

        [HttpGet("me")]
        public async Task<IActionResult> GetMine(CancellationToken ct)
        {
            var sub = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
            if (!Guid.TryParse(sub, out var userId)) return Unauthorized();

            var profile = await _db.UserProfiles.AsNoTracking().FirstOrDefaultAsync(x => x.UserId == userId, ct);
            if (profile is null)
            {
                return Ok(new ProfileDto(userId, null, null, null, null));
            }
            return Ok(new ProfileDto(profile.UserId, profile.Age, profile.Gender, profile.City, profile.AvatarUrl));
        }

        [HttpPut("me")]
        public async Task<IActionResult> UpsertMine([FromBody] UpdateProfileRequest request, CancellationToken ct)
        {
            var sub = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
            if (!Guid.TryParse(sub, out var userId)) return Unauthorized();

            var profile = await _db.UserProfiles.FirstOrDefaultAsync(x => x.UserId == userId, ct);
            if (profile is null)
            {
                profile = Domain.Users.UserProfile.Create(userId);
                profile.Update(request.Age, request.Gender, request.City, request.AvatarUrl);
                _db.UserProfiles.Add(profile);
            }
            else
            {
                profile.Update(request.Age, request.Gender, request.City, request.AvatarUrl);
            }

            await _db.SaveChangesAsync(ct);
            return Ok(new ProfileDto(userId, profile.Age, profile.Gender, profile.City, profile.AvatarUrl));
        }
    }
}


