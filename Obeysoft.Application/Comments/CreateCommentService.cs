// FILE: Obeysoft.Application/Comments/CreateCommentService.cs
using FluentValidation;
using Obeysoft.Domain.Comments;

namespace Obeysoft.Application.Comments
{
    /// <summary>
    /// Yorum oluşturma iş akışı.
    /// Controller, AuthorId’yi JWT’den alır ve servise parametre olarak gönderir.
    /// Bu servis, domain kurallarını çalıştırır ve Comment’i EF repository ile kaydeder.
    /// </summary>
    public interface ICreateCommentService
    {
        /// <summary>
        /// Yeni yorum oluşturur. Başarılıysa Id döner.
        /// </summary>
        Task<Result<Guid>> CreateAsync(CreateCommentRequestDto request, Guid authorId, CancellationToken ct);
    }

    public sealed class CreateCommentService : ICreateCommentService
    {
        private readonly ICommentRepository _repo;
        private readonly IValidator<CreateCommentRequestDto> _validator;

        public CreateCommentService(ICommentRepository repo, IValidator<CreateCommentRequestDto> validator)
        {
            _repo = repo;
            _validator = validator;
        }

        public async Task<Result<Guid>> CreateAsync(CreateCommentRequestDto request, Guid authorId, CancellationToken ct)
        {
            // 1) Validasyon
            await _validator.ValidateAndThrowAsync(request, ct);

            // 2) İş kuralları – Post var mı?
            var postExists = await _repo.PostExistsAsync(request.PostId, ct);
            if (!postExists)
                return Result<Guid>.Fail("Geçersiz PostId. Kayıtlı bir gönderi bulunamadı.");

            // 3) Parent yorum (varsa) kontrolü
            if (request.ParentId.HasValue)
            {
                var parentExists = await _repo.CommentExistsAsync(request.ParentId.Value, ct);
                if (!parentExists)
                    return Result<Guid>.Fail("Geçersiz ParentId. Üst yorum bulunamadı.");
            }

            // 4) Domain entity
            var comment = Comment.Create(
                postId: request.PostId,
                authorId: authorId,
                content: request.Content,
                parentId: request.ParentId,
                isActive: true
            );

            // 5) Kaydet
            await _repo.AddAsync(comment, ct);
            await _repo.SaveChangesAsync(ct);

            // 6) Sonuç
            return Result<Guid>.Ok(comment.Id);
        }
    }

    /// <summary>
    /// Minimal sonuç tipi. Controller’ın kullandığı IsSuccess/Value/Error alanlarını sağlar.
    /// </summary>
    public sealed class Result<T>
    {
        public bool IsSuccess { get; }
        public string? Error { get; }
        public T? Value { get; }

        private Result(bool ok, T? value, string? error)
        {
            IsSuccess = ok;
            Value = value;
            Error = error;
        }

        public static Result<T> Ok(T value) => new(true, value, null);
        public static Result<T> Fail(string error) => new(false, default, error);
    }
}