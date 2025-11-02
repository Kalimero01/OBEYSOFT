using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Obeysoft.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin")]
    public sealed class UploadsController : ControllerBase
    {
        private readonly IWebHostEnvironment _env;
        public UploadsController(IWebHostEnvironment env) => _env = env;

        public sealed class UploadRequest
        {
            public IFormFile? File { get; init; }
        }

        [HttpPost]
        [Consumes("multipart/form-data")]
        [RequestSizeLimit(1024L * 1024L * 100L)] // 100 MB
        public async Task<IActionResult> Upload([FromForm] UploadRequest form, CancellationToken ct)
        {
            var file = form.File;
            if (file == null || file.Length == 0)
                return BadRequest(new { message = "Dosya boş." });

            var uploadsRoot = Path.Combine(_env.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot"), "uploads");
            Directory.CreateDirectory(uploadsRoot);

            var fileName = $"{Guid.NewGuid():N}{Path.GetExtension(file.FileName)}";
            var fullPath = Path.Combine(uploadsRoot, fileName);

            await using (var stream = System.IO.File.Create(fullPath))
            {
                await file.CopyToAsync(stream, ct);
            }

            var req = HttpContext.Request;
            var baseUrl = $"{req.Scheme}://{req.Host}";
            var url = $"{baseUrl}/uploads/{fileName}";
            return Ok(new { url });
        }
    }
}


