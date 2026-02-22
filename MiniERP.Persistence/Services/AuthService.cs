using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MiniERP.Application.Features.Users.Queries.GetAllUsers;
using MiniERP.Application.Interfaces;
using MiniERP.Domain.Common;
using MiniERP.Persistence.IdentityModels;

namespace MiniERP.Persistence.Services;

public sealed class AuthService : IAuthService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IJwtProvider _jwtProvider;

    public AuthService(UserManager<ApplicationUser> userManager, IJwtProvider jwtProvider)
    {
        _userManager = userManager;
        _jwtProvider = jwtProvider;
    }

    public async Task<Result> AssignRoleAsync(string userId, string roleName)
    {
        // 1. Kullanıcıyı bul
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
        {
            return Result.Failure("Kullanıcı bulunamadı.");
        }

        // 2. Identity'nin kendi metoduyla kullanıcıya rolü ekle
        var result = await _userManager.AddToRoleAsync(user, roleName);

        // 3. Başarı/Hata kontrolü yap ve Result dön
        if (result.Succeeded)
        {
            return Result.Success("Kullanıcıya rol başarıyla atandı.");
        }

        var errors = result.Errors.Select(e => e.Description);
        return Result.Failure("Rol atanırken bir hata oluştu.", errors);
    }

    public async Task<Result<List<GetAllUsersQueryResponse>>> GetAllUsersAsync(CancellationToken cancellationToken)
    {
        var users = await _userManager.Users
            .Select(u => new GetAllUsersQueryResponse(
                u.Id.ToString(),
                u.FirstName,
                u.LastName,
                u.Email!,
                u.UserName!,
                true // İleride IsDeleted veya IsActive eklediğinde burayı bağlarız
            ))
            .ToListAsync(cancellationToken);

        return Result<List<GetAllUsersQueryResponse>>.Success(users, "Kullanıcı listesi başarıyla getirildi.");
    }

    public async Task<Result<string>> LoginAsync(string email, string password)
    {
        // 1. Kullanıcıyı bul
        var user = await _userManager.FindByEmailAsync(email);
        if (user == null)
        {
            return Result<string>.Failure("Kullanıcı bulunamadı veya şifre hatalı.");
        }

        // 2. Şifreyi kontrol et
        var isPasswordCorrect = await _userManager.CheckPasswordAsync(user, password);
        if (!isPasswordCorrect)
        {
            return Result<string>.Failure("Kullanıcı bulunamadı veya şifre hatalı.");
        }

        // 3. Rolleri al
        var roles = await _userManager.GetRolesAsync(user);

        // 4. Token üret
        var token = await _jwtProvider.CreateTokenAsync(user.Id.ToString(), user.Email!, roles);

        return Result<string>.Success(token, "Giriş başarılı!");
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
