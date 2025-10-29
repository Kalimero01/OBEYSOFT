// FILE: Obeysoft.Application/Comments/CreateCommentRequestDtoValidator.cs
using FluentValidation;

namespace Obeysoft.Application.Comments
{
    /// <summary>
    /// Yorum oluşturma isteği için FluentValidation kuralları.
    /// </summary>
    public sealed class CreateCommentRequestDtoValidator : AbstractValidator<CreateCommentRequestDto>
    {
        public CreateCommentRequestDtoValidator()
        {
            RuleFor(x => x.PostId)
                .NotEmpty().WithMessage("PostId zorunludur.");

            RuleFor(x => x.Content)
                .NotEmpty().WithMessage("Yorum içeriği boş olamaz.")
                .MinimumLength(3).WithMessage("Yorum çok kısa.")
                .MaximumLength(4000).WithMessage("Yorum en fazla 4000 karakter olabilir.");

            // ParentId opsiyonel; verilirse Guid.Empty olamaz
            When(x => x.ParentId.HasValue, () =>
            {
                RuleFor(x => x.ParentId)
                    .Must(id => id != Guid.Empty)
                    .WithMessage("Geçersiz ParentId.");
            });
        }
    }
}
