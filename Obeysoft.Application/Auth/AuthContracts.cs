using System;
using System.ComponentModel.DataAnnotations;
using System.Threading;
using System.Threading.Tasks;

namespace Obeysoft.Application.Auth
{
    /// <summary>
    /// Register/Login istek ve yanıt DTO'ları + kimlik doğrulama sözleşmesi.
    /// Uygulama katmanında sadece kontratlar/DTO'lar bulunur; implementasyon Infrastructure'dadır.
    /// </summary>
    public static class AuthDtos
    {
        public sealed class RegisterRequestDto
        {
            [Required, EmailAddress, MaxLength(256)]
            public string Email { get; init; } = default!;

            [Required, MinLength(2), MaxLength(100)]
            public string DisplayName { get; init; } = default!;

            [Required, MinLength(6), MaxLength(128)]
            public string Password { get; init; } = default!;
        }

        public sealed class LoginRequestDto
        {
            [Required, EmailAddress, MaxLength(256)]
            public string Email { get; init; } = default!;

            [Required, MinLength(6), MaxLength(128)]
            public string Password { get; init; } = default!;
        }

        /// <summary>
        /// Başarılı login/register sonrasında dönen token + kullanıcı özeti.
        /// </summary>
        public sealed class AuthResponseDto
        {
            public string AccessToken { get; init; } = default!;
            public DateTimeOffset ExpiresAt { get; init; }
            public Guid UserId { get; init; }
            public string Email { get; init; } = default!;
            public string DisplayName { get; init; } = default!;
            public string Role { get; init; } = default!;
        }

        /// <summary>
        /// /api/me gibi uçlar için minimal kimlik özeti.
        /// </summary>
        public sealed class MeDto
        {
            public Guid UserId { get; init; }
            public string Email { get; init; } = default!;
            public string DisplayName { get; init; } = default!;
            public string Role { get; init; } = default!;
        }
    }

    /// <summary>
    /// Kimlik doğrulama sözleşmesi. Implementasyon Infrastructure katmanında yapılır.
    /// </summary>
    public interface IAuthService
    {
        Task<AuthDtos.AuthResponseDto> RegisterAsync(
            AuthDtos.RegisterRequestDto request,
            CancellationToken ct);

        Task<AuthDtos.AuthResponseDto> LoginAsync(
            AuthDtos.LoginRequestDto request,
            CancellationToken ct);

        Task<AuthDtos.MeDto?> GetMeAsync(Guid userId, CancellationToken ct);
    }
}
