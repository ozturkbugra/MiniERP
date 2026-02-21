using Microsoft.AspNetCore.Identity;
using MiniERP.Application.Interfaces;
using MiniERP.Domain.Common;
using MiniERP.Persistence.IdentityModels;

namespace MiniERP.Persistence.Services;

public sealed class AuthService : IAuthService
{
    private readonly UserManager<ApplicationUser> _userManager;

    public AuthService(UserManager<ApplicationUser> userManager)
    {
        _userManager = userManager;
    }

    public async Task<Result> RegisterAsync(string firstName, string lastName, string email, string password)
    {
        // Yeni bir kullanıcı nesnesi oluştur (Gölge Modelimiz)
        var user = new ApplicationUser
        {
            Id = Guid.NewGuid(),
            FirstName = firstName,
            LastName = lastName,
            Email = email,
            UserName = email
        };

        // Kullanıcıyı oluştur (Şifreyi Identity kendisi hash'leyip saklayacak)
        var result = await _userManager.CreateAsync(user, password);

        // Başarılı mı kontrol et
        if (result.Succeeded)
        {
            return Result.Success("Kullanıcı kaydı başarıyla tamamlandı");
        }

        // Hata varsa hataları topla ve dön
        var errors = result.Errors.Select(e => e.Description);
        return Result.Failure("Kullanıcı oluşturulurken bir hata meydana geldi.", errors);
    }
}