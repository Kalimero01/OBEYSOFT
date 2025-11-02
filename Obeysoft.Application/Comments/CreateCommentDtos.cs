// FILE: Obeysoft.Application/Comments/CreateCommentDtos.cs
using System.ComponentModel.DataAnnotations;

namespace Obeysoft.Application.Comments
{
    // İstek DTO: Üye, bir posta yorum bırakır.
    // AuthorId JWT'den alınacak; client göndermeyecek.
    public sealed class CreateCommentRequestDto
    {
        /// <summary>Yorum yapılacak Post Id</summary>
        [Required]
        public Guid PostId { get; init; }

        /// <summary>İsteğe bağlı: Cevaplanan yorum Id (threading)</summary>
        public Guid? ParentId { get; init; }

        /// <summary>Yorum metni</summary>
        [Required]
        public string Content { get; init; } = string.Empty;
    }

    // Yanıt DTO: Oluşan yorumun Id'si ve temel bilgileri
    public sealed class CreateCommentResponseDto
    {
        public Guid Id { get; init; }
        public Guid PostId { get; init; }
        public Guid? ParentId { get; init; }
        public string Content { get; init; } = string.Empty;
        public DateTimeOffset CreatedAt { get; init; }
        public bool IsApproved { get; init; }
    }
}
