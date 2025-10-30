using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Obeysoft.Application.Comments;
using System.Security.Claims;

namespace Obeysoft.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public sealed class CommentsController : ControllerBase
    {
        private readonly ICreateCommentService _createService;

        public CommentsController(ICreateCommentService createService)
        {
            _createService = createService;
        }

        /// <summary>
        /// Yeni yorum oluştur (sadece giriş yapmış kullanıcılar).
        /// </summary>
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> Create([FromBody] CreateCommentRequestDto dto, CancellationToken ct)
        {
            var authorIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (authorIdClaim is null)
                return Unauthorized(new { message = "Token geçersiz." });

            var authorId = Guid.Parse(authorIdClaim);
            var result = await _createService.CreateAsync(dto, authorId, ct);

            if (!result.IsSuccess)
                return BadRequest(new { message = result.Error });

            return Ok(new { id = result.Value });
        }
    }
}