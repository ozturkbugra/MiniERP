using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MiniERP.Application.Features.Users.Commands.UpdateUser;
using MiniERP.Application.Features.Users.Queries.GetAllUsers;
using MiniERP.Application.Features.Users.Queries.GetUserById;
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
        if (user == null || user.IsDeleted)
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


    public async Task<Result<string>> RemoveRoleFromUserAsync(string userId, string roleName)
    {
        // 1. Kullanıcıyı bul
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null || user.IsDeleted)
            return Result<string>.Failure("Kullanıcı bulunamadı.");

        var isInRole = await _userManager.IsInRoleAsync(user, roleName);
        if (!isInRole)
            return Result<string>.Failure("Kullanıcı zaten bu role sahip değil.");

        var result = await _userManager.RemoveFromRoleAsync(user, roleName);

        if (result.Succeeded)
        {
            return Result<string>.Success(user.Id.ToString(), $"'{roleName}' rolü kullanıcıdan başarıyla silindi.");
        }

        return Result<string>.Failure("Rol çıkarılırken bir hata oluştu: " +
            string.Join(", ", result.Errors.Select(e => e.Description)));
    }

    public async Task<Result<List<GetAllUsersQueryResponse>>> GetAllUsersAsync(CancellationToken cancellationToken)
    {
        var users = await _userManager.Users
            .Where(u => !u.IsDeleted)
            .Select(u => new GetAllUsersQueryResponse(
                u.Id.ToString(),
                u.FirstName,
                u.LastName,
                u.Email!,
                u.UserName!,
                !u.IsDeleted
            ))
            .ToListAsync(cancellationToken);

        return Result<List<GetAllUsersQueryResponse>>.Success(users, "Kullanıcı listesi başarıyla getirildi.");
    }

    public async Task<Result<GetUserByIdQueryResponse>> GetUserByIdAsync(string id, CancellationToken cancellationToken)
    {
        var user = await _userManager.FindByIdAsync(id);

        if (user is null || user.IsDeleted)
            return Result<GetUserByIdQueryResponse>.Failure("Kullanıcı bulunamadı.");

        var response = new GetUserByIdQueryResponse(
            user.Id.ToString(),
            user.FirstName,
            user.LastName,
            user.Email!,
            user.UserName!,
            !user.IsDeleted);

        return Result<GetUserByIdQueryResponse>.Success(response, "Kullanıcı bilgileri başarıyla getirildi.");
    }

    public async Task<Result<string>> LoginAsync(string email, string password)
    {
        var user = await _userManager.FindByEmailAsync(email);
        if (user == null || user.IsDeleted)
        {
            return Result<string>.Failure("Kullanıcı bulunamadı veya şifre hatalı.");
        }

        var isPasswordCorrect = await _userManager.CheckPasswordAsync(user, password);
        if (!isPasswordCorrect)
        {
            return Result<string>.Failure("Kullanıcı bulunamadı veya şifre hatalı.");
        }

        var roles = await _userManager.GetRolesAsync(user);
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

    public async Task<Result<string>> UpdateUserAsync(UpdateUserCommand request, CancellationToken cancellationToken)
    {
        var user = await _userManager.FindByIdAsync(request.Id);

        if (user is null || user.IsDeleted)
            return Result<string>.Failure("Kullanıcı bulunamadı.");

        user.FirstName = request.FirstName;
        user.LastName = request.LastName;
        user.Email = request.Email;
        user.UserName = request.UserName;

        var result = await _userManager.UpdateAsync(user);

        if (result.Succeeded)
        {
            return Result<string>.Success(user.Id.ToString(), "Kullanıcı bilgileri başarıyla güncellendi.");
        }

        return Result<string>.Failure(string.Join(", ", result.Errors.Select(e => e.Description)));
    }

    public async Task<Result<string>> DeleteUserAsync(string id, CancellationToken cancellationToken)
    {
        var user = await _userManager.FindByIdAsync(id);

        if (user is null || user.IsDeleted) 
            return Result<string>.Failure("Kullanıcı bulunamadı.");

        user.IsDeleted = true;

        var result = await _userManager.UpdateAsync(user);

        if (result.Succeeded)
        {
            return Result<string>.Success(user.Id.ToString(), "Kullanıcı başarıyla pasife çekildi (Soft Delete).");
        }

        return Result<string>.Failure(string.Join(", ", result.Errors.Select(e => e.Description)));
    }
}
