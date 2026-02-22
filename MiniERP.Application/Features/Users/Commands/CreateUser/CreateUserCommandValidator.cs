using FluentValidation;

namespace MiniERP.Application.Features.Users.Commands.CreateUser;

public sealed class CreateUserCommandValidator : AbstractValidator<CreateUserCommand>
{
    public CreateUserCommandValidator()
    {
        RuleFor(p => p.FirstName)
            .NotEmpty().WithMessage("Ad alanı boş geçilemez.")
            .MinimumLength(2).WithMessage("Ad en az 2 karakter olmalıdır.");

        RuleFor(p => p.LastName)
            .NotEmpty().WithMessage("Soyad alanı boş geçilemez.")
            .MinimumLength(2).WithMessage("Soyad en az 2 karakter olmalıdır.");

        RuleFor(p => p.Email)
            .NotEmpty().WithMessage("Email alanı boş geçilemez.")
            .EmailAddress().WithMessage("Geçerli bir email adresi giriniz.");

        RuleFor(p => p.Password)
            .NotEmpty().WithMessage("Şifre boş geçilemez.")
            .MinimumLength(6).WithMessage("Şifre en az 6 karakter olmalıdır.")
            .Matches("[A-Z]").WithMessage("Şifre en az bir büyük harf içermelidir.")
            .Matches("[a-z]").WithMessage("Şifre en az bir küçük harf içermelidir.")
            .Matches("[0-9]").WithMessage("Şifre en az bir rakam içermelidir.");
    }
}