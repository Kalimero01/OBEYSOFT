// FILE: Obeysoft.Api/Controllers/PostsController.cs
using System;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Obeysoft.Application.Posts;

namespace Obeysoft.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public sealed class PostsController : ControllerBase
    {
        private readonly IGetPostService _getService;
        private readonly IManagePostService _manageService;

        public PostsController(IGetPostService getService, IManagePostService manageService)
        {
            _getService = getService;
            _manageService = manageService;
        }

        [HttpGet]
        [ProducesResponseType(typeof(PagedResult<GetPostListItemDto>), 200)]
        public async Task<IActionResult> GetPublished(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string? category = null,
            [FromQuery] string? search = null,
            CancellationToken ct = default)
        {
            var result = await _getService.GetPublishedAsync(page, pageSize, category, search, ct);
            return Ok(result);
        }

        [HttpGet("{slug}")]
        [ProducesResponseType(typeof(GetPostDetailDto), 200)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> GetBySlug(string slug, CancellationToken ct)
        {
            var dto = await _getService.GetBySlugAsync(slug, ct);
            return dto is null ? NotFound() : Ok(dto);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(typeof(PostSavedDto), 200)]
        public async Task<IActionResult> Create([FromBody] CreatePostRequestDto dto, CancellationToken ct)
        {
            var userId = GetCurrentUserId();
            var created = await _manageService.CreateAsync(dto, userId, ct);
            return Ok(created);
        }

        [HttpPut("{id:guid}")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(typeof(PostSavedDto), 200)]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdatePostRequestDto dto, CancellationToken ct)
        {
            var fixedDto = new UpdatePostRequestDto
            {
                Id = id,
                Title = dto.Title,
                Slug = dto.Slug,
                Content = dto.Content,
                CategoryId = dto.CategoryId,
                Summary = dto.Summary,
                IsActive = dto.IsActive
            };

            var userId = GetCurrentUserId();
            var updated = await _manageService.UpdateAsync(fixedDto, userId, ct);
            return Ok(updated);
        }

        [HttpPost("{id:guid}/publish")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(typeof(PostSavedDto), 200)]
        public async Task<IActionResult> Publish(Guid id, CancellationToken ct)
        {
            var userId = GetCurrentUserId();
            var published = await _manageService.PublishAsync(id, userId, ct);
            return Ok(published);
        }

        [HttpPost("{id:guid}/unpublish")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(typeof(PostSavedDto), 200)]
        public async Task<IActionResult> Unpublish(Guid id, CancellationToken ct)
        {
            var userId = GetCurrentUserId();
            var unpublished = await _manageService.UnpublishAsync(id, userId, ct);
            return Ok(unpublished);
        }

        [HttpDelete("{id:guid}")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(200)]
        public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
        {
            var userId = GetCurrentUserId();
            await _manageService.DeleteAsync(id, userId, ct);
            return Ok(new { message = "Post silindi." });
        }

        private Guid GetCurrentUserId()
        {
            var sub = User.FindFirstValue(ClaimTypes.NameIdentifier)
                      ?? User.FindFirstValue("sub");

            if (string.IsNullOrWhiteSpace(sub))
                throw new InvalidOperationException("JWT içinde kullanıcı kimliği yok.");

            return Guid.Parse(sub);
        }
    }
}
