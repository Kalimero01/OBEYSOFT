using System;

namespace Obeysoft.Domain.Users
{
    /// <summary>
    /// Kullanıcı rolleri (genişletilebilir).
    /// </summary>
    public enum UserRole
    {
        Member = 0,
        Admin = 1
    }

    /// <summary>
    /// Sistem kullanıcısı (Domain entity).
    /// Not: Parola HASH üretimi Domain'in görevi değildir; sadece hash/salt alanlarını tutar.
    /// </summary>
    public sealed class User
    {
        // Kimlik
        public Guid Id { get; private set; }

        // Hesap bilgileri
        public string Email { get; private set; } = default!;
        public string DisplayName { get; private set; } = default!;
        public UserRole Role { get; private set; }

        // Güvenlik
        public string PasswordHash { get; private set; } = default!;
        public string PasswordSalt { get; private set; } = default!;
        public DateTimeOffset? LastLoginAt { get; private set; }

        // Durum
        public bool IsActive { get; private set; }

        // Audit
        public DateTimeOffset CreatedAt { get; private set; }
        public DateTimeOffset? UpdatedAt { get; private set; }

        // EF/Serileştirme için parametresiz ctor
        private User() { }

        private User(Guid id, string email, string displayName, UserRole role, bool isActive)
        {
            Id = id == Guid.Empty ? Guid.NewGuid() : id;
            SetEmail(email);
            SetDisplayName(displayName);
            Role = role;
            IsActive = isActive;
            CreatedAt = DateTimeOffset.UtcNow;
        }

        // -------- Fabrikalar --------
        public static User CreateNew(string email, string displayName, UserRole role = UserRole.Member, bool isActive = true)
            => new User(Guid.NewGuid(), email, displayName, role, isActive);

        // -------- Davranışlar --------
        public void UpdateProfile(string displayName)
        {
            SetDisplayName(displayName);
            Touch();
        }

        public void Activate()
        {
            if (!IsActive) { IsActive = true; Touch(); }
        }

        public void Deactivate()
        {
            if (IsActive) { IsActive = false; Touch(); }
        }

        public void PromoteToAdmin()
        {
            if (Role != UserRole.Admin) { Role = UserRole.Admin; Touch(); }
        }

        public void DemoteToMember()
        {
            if (Role != UserRole.Member) { Role = UserRole.Member; Touch(); }
        }

        /// <summary>
        /// Parola hash/salt değerlerini Infrastructure katmanı belirler ve buraya set eder.
        /// </summary>
        public void SetPasswordSecret(string passwordHash, string passwordSalt)
        {
            if (string.IsNullOrWhiteSpace(passwordHash)) throw new ArgumentException("passwordHash boş olamaz.", nameof(passwordHash));
            if (string.IsNullOrWhiteSpace(passwordSalt)) throw new ArgumentException("passwordSalt boş olamaz.", nameof(passwordSalt));
            PasswordHash = passwordHash;
            PasswordSalt = passwordSalt;
            Touch();
        }

        public void MarkLogin()
        {
            LastLoginAt = DateTimeOffset.UtcNow;
            Touch();
        }

        // -------- İç doğrulamalar --------
        private void SetEmail(string email)
        {
            email = (email ?? string.Empty).Trim();
            if (email.Length < 5 || !email.Contains("@")) throw new ArgumentException("Geçerli bir e-posta gereklidir.", nameof(email));
            if (email.Length > 256) throw new ArgumentException("E-posta 256 karakteri aşamaz.", nameof(email));
            Email = email.ToLowerInvariant();
        }

        private void SetDisplayName(string name)
        {
            name = (name ?? string.Empty).Trim();
            if (name.Length < 2) throw new ArgumentException("Görünen ad en az 2 karakter olmalıdır.", nameof(name));
            if (name.Length > 100) throw new ArgumentException("Görünen ad 100 karakteri aşamaz.", nameof(name));
            DisplayName = name;
        }

        private void Touch() => UpdatedAt = DateTimeOffset.UtcNow;
    }
}
