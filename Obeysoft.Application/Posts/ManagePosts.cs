// FILE: Obeysoft.Application/Posts/ManagePosts.cs
using System;
using System.ComponentModel.DataAnnotations;
using System.Threading;
using System.Threading.Tasks;

namespace Obeysoft.Application.Posts
{
    /// <summary>
    /// Yeni bir blog yazısı / içerik oluşturmak için kullanılacak DTO.
    /// API bu modeli alacak → Application servis bunu işleyecek.
    /// </summary>
    public sealed class CreatePostRequestDto
    {
        [Required, MinLength(3), MaxLength(180)]
        public string Title { get; init; } = string.Empty;

        /// <summary>
        /// Opsiyonel. Gönderilmezse servis başlıktan slug üretir.
        /// </summary>
        [MaxLength(180)]
        public string? Slug { get; init; }

        [Required, MinLength(10)]
        public string Content { get; init; } = string.Empty;

        /// <summary>Hangi kategoriye ait olduğu</summary>
        [Required]
        public Guid CategoryId { get; init; }

        /// <summary>Liste sayfasında gösterilecek kısa özet (opsiyonel)</summary>
        [MaxLength(400)]
        public string? Summary { get; init; }

        /// <summary>İçerik oluşturulurken hemen yayınlansın mı?</summary>
        public bool PublishNow { get; init; } = false;
    }

    /// <summary>
    /// Var olan bir blog yazısını güncellemek için kullanılacak DTO.
    /// </summary>
    public sealed class UpdatePostRequestDto
    {
        [Required]
        public Guid Id { get; init; }

        [Required, MinLength(3), MaxLength(180)]
        public string Title { get; init; } = string.Empty;

        [MaxLength(180)]
        public string? Slug { get; init; }

        [Required, MinLength(10)]
        public string Content { get; init; } = string.Empty;

        [Required]
        public Guid CategoryId { get; init; }

        [MaxLength(400)]
        public string? Summary { get; init; }

        /// <summary>Admin pasife çekmek isterse.</summary>
        public bool IsActive { get; init; } = true;
    }

    /// <summary>
    /// Create / Update / Publish işlemlerinden sonra döneceğimiz özet DTO.
    /// </summary>
    public sealed class PostSavedDto
    {
        public Guid Id { get; init; }
        public string Title { get; init; } = string.Empty;
        public string Slug { get; init; } = string.Empty;
        public Guid CategoryId { get; init; }
        public bool IsPublished { get; init; }
        public DateTimeOffset? PublishedAt { get; init; }
    }

    /// <summary>
    /// Post yönetimi için application katmanındaki sözleşme.
    /// Burada sadece İMZA var, EF/DbContext yok. Onlar Infrastructure’da olacak.
    /// </summary>
    public interface IManagePostService
    {
        Task<PostSavedDto> CreateAsync(CreatePostRequestDto request, Guid actorUserId, CancellationToken ct);
        Task<PostSavedDto> UpdateAsync(UpdatePostRequestDto request, Guid actorUserId, CancellationToken ct);
        Task<PostSavedDto> PublishAsync(Guid postId, Guid actorUserId, CancellationToken ct);
        Task<PostSavedDto> UnpublishAsync(Guid postId, Guid actorUserId, CancellationToken ct);
        Task DeleteAsync(Guid postId, Guid actorUserId, CancellationToken ct);
    }
}
