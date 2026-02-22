using FluentValidation;

namespace MiniERP.Application.Features.Users.Commands.ChangePassword;

public sealed class ChangePasswordCommandValidator : AbstractValidator<ChangePasswordCommand>
{
    public ChangePasswordCommandValidator()
    {
        // 1. Kimin şifresi değişecek?
        RuleFor(p => p.UserId)
            .NotEmpty().WithMessage("Kullanıcı ID alanı boş geçilemez.");

        // 2. Güvenlik teyidi için eski şifre
        RuleFor(p => p.OldPassword)
            .NotEmpty().WithMessage("Mevcut şifrenizi girmek zorundasınız.");

        // 3. Yeni şifre ve güvenlik kuralları
        RuleFor(p => p.NewPassword)
            .NotEmpty().WithMessage("Yeni şifre boş geçilemez.")
            .MinimumLength(6).WithMessage("Yeni şifre en az 6 karakter olmalıdır.")
            .Matches("[A-Z]").WithMessage("Yeni şifre en az bir büyük harf içermelidir.")
            .Matches("[a-z]").WithMessage("Yeni şifre en az bir küçük harf içermelidir.")
            .Matches("[0-9]").WithMessage("Yeni şifre en az bir rakam içermelidir.")
            .NotEqual(p => p.OldPassword).WithMessage("Yeni şifre, eski şifrenizle aynı olamaz.");
    }
}