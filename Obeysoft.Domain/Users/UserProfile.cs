using System;

namespace Obeysoft.Domain.Users
{
    /// <summary>
    /// Kullanıcıya bağlı profil bilgileri (isteğe bağlı alanlar dahil).
    /// </summary>
    public sealed class UserProfile
    {
        public Guid Id { get; private set; }
        public Guid UserId { get; private set; }

        public int? Age { get; private set; }
        public string? Gender { get; private set; }
        public string? City { get; private set; }
        public string? AvatarUrl { get; private set; }

        public DateTimeOffset CreatedAt { get; private set; }
        public DateTimeOffset? UpdatedAt { get; private set; }

        private UserProfile() { }

        private UserProfile(Guid userId)
        {
            Id = Guid.NewGuid();
            UserId = userId;
            CreatedAt = DateTimeOffset.UtcNow;
        }

        public static UserProfile Create(Guid userId) => new UserProfile(userId);

        public void Update(int? age, string? gender, string? city, string? avatarUrl)
        {
            if (age is not null && (age < 0 || age > 120))
                throw new ArgumentException("Geçerli bir yaş giriniz.", nameof(age));

            Age = age;
            Gender = string.IsNullOrWhiteSpace(gender) ? null : gender.Trim();
            City = string.IsNullOrWhiteSpace(city) ? null : city.Trim();
            AvatarUrl = string.IsNullOrWhiteSpace(avatarUrl) ? null : avatarUrl.Trim();
            UpdatedAt = DateTimeOffset.UtcNow;
        }
    }
}


