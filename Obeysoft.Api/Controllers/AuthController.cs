using System;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Obeysoft.Application.Auth;
using static Obeysoft.Application.Auth.AuthDtos;

namespace Obeysoft.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public sealed class AuthController : ControllerBase
    {
        private readonly IAuthService _auth;

        public AuthController(IAuthService auth) => _auth = auth;

        /// <summary>Yeni kullanıcı oluşturur ve JWT döner.</summary>
        [HttpPost("register")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(AuthResponseDto), 200)]
        [ProducesResponseType(typeof(ValidationProblemDetails), 400)]
        public async Task<IActionResult> Register([FromBody] RegisterRequestDto request, CancellationToken ct)
        {
            if (!ModelState.IsValid) return ValidationProblem(ModelState);
            try
            {
                var res = await _auth.RegisterAsync(request, ct);
                return Ok(res);
            }
            catch (InvalidOperationException ex)
            {
                // örn: e-posta zaten kayıtlı
                return Conflict(new ProblemDetails
                {
                    Title = "Kayıt oluşturulamadı",
                    Detail = ex.Message,
                    Status = 409
                });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new ProblemDetails
                {
                    Title = "Geçersiz istek",
                    Detail = ex.Message,
                    Status = 400
                });
            }
        }

        /// <summary>Kullanıcı girişi yapar ve JWT döner.</summary>
        [HttpPost("login")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(AuthResponseDto), 200)]
        [ProducesResponseType(typeof(ProblemDetails), 401)]
        public async Task<IActionResult> Login([FromBody] LoginRequestDto request, CancellationToken ct)
        {
            if (!ModelState.IsValid) return ValidationProblem(ModelState);
            try
            {
                var res = await _auth.LoginAsync(request, ct);
                return Ok(res);
            }
            catch (InvalidOperationException ex)
            {
                return Unauthorized(new ProblemDetails
                {
                    Title = "Giriş başarısız",
                    Detail = ex.Message,
                    Status = 401
                });
            }
        }

        /// <summary>Aktif kullanıcının özet bilgisi (JWT zorunlu).</summary>
        [HttpGet("me")]
        [Authorize]
        [ProducesResponseType(typeof(MeDto), 200)]
        [ProducesResponseType(401)]
        public async Task<IActionResult> Me(CancellationToken ct)
        {
            var sub = User.FindFirstValue(ClaimTypes.NameIdentifier)
                      ?? User.FindFirstValue("sub"); // fallback

            if (!Guid.TryParse(sub, out var userId))
                return Unauthorized();

            var me = await _auth.GetMeAsync(userId, ct);
            return me is null ? Unauthorized() : Ok(me);
        }
    }
}