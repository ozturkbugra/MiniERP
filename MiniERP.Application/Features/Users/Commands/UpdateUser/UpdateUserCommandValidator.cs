using FluentValidation;

namespace MiniERP.Application.Features.Users.Commands.UpdateUser;

public sealed class UpdateUserCommandValidator : AbstractValidator<UpdateUserCommand>
{
    public UpdateUserCommandValidator()
    {
        // Güncelleme işleminde en kritik kural: Hangi kullanıcı güncellenecek?
        RuleFor(p => p.Id)
            .NotEmpty().WithMessage("Güncellenecek kullanıcı ID'si boş olamaz.");

        RuleFor(p => p.FirstName)
            .NotEmpty().WithMessage("Ad alanı boş geçilemez.")
            .MinimumLength(2).WithMessage("Ad en az 2 karakter olmalıdır.");

        RuleFor(p => p.LastName)
            .NotEmpty().WithMessage("Soyad alanı boş geçilemez.")
            .MinimumLength(2).WithMessage("Soyad en az 2 karakter olmalıdır.");

        RuleFor(p => p.Email)
            .NotEmpty().WithMessage("Email alanı boş geçilemez.")
            .EmailAddress().WithMessage("Geçerli bir email adresi giriniz.");

        // Eğer Update modelinde UserName de güncelleniyorsa:
        RuleFor(p => p.UserName)
            .NotEmpty().WithMessage("Kullanıcı adı boş geçilemez.")
            .MinimumLength(3).WithMessage("Kullanıcı adı en az 3 karakter olmalıdır.");
    }
}