using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Obeysoft.Application.Posts;

namespace Obeysoft.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public sealed class PostsController : ControllerBase
    {
        private readonly IGetPostService _service;

        public PostsController(IGetPostService service) => _service = service;

        /// <summary>
        /// Yayındaki postları sayfalı döndürür. En yeni yayımlananlar üstte.
        /// Query: page (>=1), pageSize (1-100), category (slug), search (metin).
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(PagedResult<GetPostListItemDto>), 200)]
        public async Task<IActionResult> GetPublished(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string? category = null,
            [FromQuery] string? search = null,
            CancellationToken ct = default)
        {
            var result = await _service.GetPublishedAsync(page, pageSize, category, search, ct);
            return Ok(result);
        }

        /// <summary>Slug'a göre yayımlanmış post detayını döndürür.</summary>
        [HttpGet("{slug}")]
        [ProducesResponseType(typeof(GetPostDetailDto), 200)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> GetBySlug(string slug, CancellationToken ct)
        {
            var dto = await _service.GetBySlugAsync(slug, ct);
            return dto is null ? NotFound() : Ok(dto);
        }
    }
}
