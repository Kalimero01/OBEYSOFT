using System;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Obeysoft.Application.Auth;
using Obeysoft.Domain.Users;
using Obeysoft.Infrastructure.Persistence;
using Npgsql;

namespace Obeysoft.Infrastructure.Auth
{
    public sealed class AuthService : IAuthService
    {
        private readonly BlogDbContext _db;
        private readonly IConfiguration _config;

        public AuthService(BlogDbContext db, IConfiguration config)
        {
            _db = db;
            _config = config;
        }

        public async Task<AuthDtos.AuthResponseDto> RegisterAsync(AuthDtos.RegisterRequestDto request, CancellationToken ct)
        {
            var email = request.Email.Trim().ToLowerInvariant();

            if (await _db.Users.AnyAsync(u => u.Email == email, ct))
                throw new InvalidOperationException("Bu e-posta ile bir kullanıcı zaten mevcut.");

            var user = User.CreateNew(email, request.DisplayName, UserRole.Member, isActive: true);

            var (hash, salt) = PasswordHasher.Hash(request.Password);
            user.SetPasswordSecret(hash, salt);

            _db.Users.Add(user);

            try
            {
                await _db.SaveChangesAsync(ct);
            }
            catch (PostgresException pex) when (pex.SqlState == "23505")
            {
                throw new InvalidOperationException("Bu e-posta ile bir kullanıcı zaten mevcut.");
            }
            catch (DbUpdateException ex) when (ex.InnerException is PostgresException pex && pex.SqlState == "23505")
            {
                throw new InvalidOperationException("Bu e-posta ile bir kullanıcı zaten mevcut.");
            }
            catch (PostgresException)
            {
                throw new InvalidOperationException("Geçici bir veritabanı hatası oluştu, lütfen tekrar deneyin.");
            }

            var (token, expires) = CreateJwt(user);
            return new AuthDtos.AuthResponseDto
            {
                AccessToken = token,
                ExpiresAt = expires,
                UserId = user.Id,
                Email = user.Email,
                DisplayName = user.DisplayName,
                Role = user.Role.ToString()
            };
        }

        public async Task<AuthDtos.AuthResponseDto> LoginAsync(AuthDtos.LoginRequestDto request, CancellationToken ct)
        {
            var email = request.Email.Trim().ToLowerInvariant();
            var user = await _db.Users.FirstOrDefaultAsync(u => u.Email == email, ct);
            if (user is null || !user.IsActive)
                throw new InvalidOperationException("Kullanıcı bulunamadı veya pasif.");

            if (!PasswordHasher.Verify(request.Password, user.PasswordHash, user.PasswordSalt))
                throw new InvalidOperationException("Geçersiz e-posta/parola.");

            // Login history’yi güncelle; geçici DB hatası olursa login’i düşürme.
            user.MarkLogin();
            try
            {
                await _db.SaveChangesAsync(ct);
            }
            catch (PostgresException)
            {
                // Yalnızca login zaman damgasını kaydedemedik; token üretmeye devam.
            }
            catch (DbUpdateException ex) when (ex.InnerException is PostgresException)
            {
                // Aynı şekilde devam.
            }

            var (token, expires) = CreateJwt(user);
            return new AuthDtos.AuthResponseDto
            {
                AccessToken = token,
                ExpiresAt = expires,
                UserId = user.Id,
                Email = user.Email,
                DisplayName = user.DisplayName,
                Role = user.Role.ToString()
            };
        }

        public async Task<AuthDtos.MeDto?> GetMeAsync(Guid userId, CancellationToken ct)
        {
            var u = await _db.Users.AsNoTracking().FirstOrDefaultAsync(x => x.Id == userId, ct);
            if (u is null) return null;

            return new AuthDtos.MeDto
            {
                UserId = u.Id,
                Email = u.Email,
                DisplayName = u.DisplayName,
                Role = u.Role.ToString()
            };
        }

        private (string token, DateTimeOffset expiresAt) CreateJwt(User user)
        {
            var jwtSection = _config.GetSection("Jwt");
            var key = jwtSection.GetValue<string>("Key") ?? throw new InvalidOperationException("Jwt:Key boş olamaz.");
            var issuer = jwtSection.GetValue<string>("Issuer") ?? "Obeysoft";
            var audience = jwtSection.GetValue<string>("Audience") ?? "ObeysoftClient";
            var minutes = jwtSection.GetValue<int?>("AccessTokenMinutes") ?? 60;

            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));
            var creds = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var now = DateTimeOffset.UtcNow;
            var expires = now.AddMinutes(minutes);

            var claims = new[]
            {
                new System.Security.Claims.Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                new System.Security.Claims.Claim(JwtRegisteredClaimNames.Email, user.Email),
                new System.Security.Claims.Claim("name", user.DisplayName),
                new System.Security.Claims.Claim("role", user.Role.ToString()),
                new System.Security.Claims.Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            var token = new JwtSecurityToken(
                issuer: issuer,
                audience: audience,
                claims: claims,
                notBefore: now.UtcDateTime,
                expires: expires.UtcDateTime,
                signingCredentials: creds);

            var jwt = new JwtSecurityTokenHandler().WriteToken(token);
            return (jwt, expires);
        }

        private static class PasswordHasher
        {
            private const int SaltSize = 32;
            private const int HashSize = 32;
            private const int Iterations = 100_000;

            public static (string hash, string salt) Hash(string password)
            {
                if (string.IsNullOrWhiteSpace(password))
                    throw new ArgumentException("Parola boş olamaz.", nameof(password));

                Span<byte> salt = stackalloc byte[SaltSize];
                RandomNumberGenerator.Fill(salt);
                using var pbkdf2 = new Rfc2898DeriveBytes(password, salt.ToArray(), Iterations, HashAlgorithmName.SHA256);
                var hash = pbkdf2.GetBytes(HashSize);

                return (Convert.ToBase64String(hash), Convert.ToBase64String(salt.ToArray()));
            }

            public static bool Verify(string password, string storedHashBase64, string storedSaltBase64)
            {
                if (string.IsNullOrWhiteSpace(storedHashBase64) || string.IsNullOrWhiteSpace(storedSaltBase64))
                    return false;

                var salt = Convert.FromBase64String(storedSaltBase64);
                using var pbkdf2 = new Rfc2898DeriveBytes(password, salt, Iterations, HashAlgorithmName.SHA256);
                var computed = pbkdf2.GetBytes(HashSize);
                var stored = Convert.FromBase64String(storedHashBase64);

                return CryptographicOperations.FixedTimeEquals(computed, stored);
            }
        }
    }
}
