using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MiniERP.Application.Features.Users.Commands.ChangePassword;
using MiniERP.Application.Features.Users.Commands.UpdateUser;
using MiniERP.Application.Features.Users.Queries.GetAllUsers;
using MiniERP.Application.Features.Users.Queries.GetUserById;
using MiniERP.Application.Interfaces;
using MiniERP.Domain.Common;
using MiniERP.Domain.Entities;
using MiniERP.Persistence.Context;
using MiniERP.Persistence.IdentityModels;
using System.IdentityModel.Tokens.Jwt;

namespace MiniERP.Persistence.Services;

public sealed class AuthService : IAuthService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IJwtProvider _jwtProvider;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IUnitOfWork _unitOfWork;
    private readonly AppDbContext _context;

    public AuthService(UserManager<ApplicationUser> userManager, IJwtProvider jwtProvider, IHttpContextAccessor httpContextAccessor, IUnitOfWork unitOfWork, AppDbContext context)
    {
        _userManager = userManager;
        _jwtProvider = jwtProvider;
        _httpContextAccessor = httpContextAccessor;
        _unitOfWork = unitOfWork;
        _context = context;
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
            !u.IsDeleted,
            _userManager.GetRolesAsync(u).Result.ToList()
        ))
        .ToListAsync(cancellationToken);

        return Result<List<GetAllUsersQueryResponse>>.Success(users, "Kullanıcılar ve rolleri başarıyla getirildi.");
    }

    public async Task<Result<GetUserByIdQueryResponse>> GetUserByIdAsync(string id, CancellationToken cancellationToken)
    {
        var user = await _userManager.FindByIdAsync(id);

        if (user is null || user.IsDeleted)
            return Result<GetUserByIdQueryResponse>.Failure("Kullanıcı bulunamadı.");

        var roles = await _userManager.GetRolesAsync(user);

        var response = new GetUserByIdQueryResponse(
            user.Id.ToString(),
            user.FirstName,
            user.LastName,
            user.Email!,
            user.UserName!,
            !user.IsDeleted,
            roles.ToList()); 

        return Result<GetUserByIdQueryResponse>.Success(response, "Kullanıcı bilgileri ve rolleri başarıyla getirildi.");
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

        // 1. Kullanıcının rollerini al
        var roles = await _userManager.GetRolesAsync(user);

        // 2. BEYİN BURASI: Bu rollere ait ID'leri bul 
        var roleIds = await _context.Roles
            .Where(r => roles.Contains(r.Name!))
            .Select(r => r.Id)
            .ToListAsync();

        // 3. Role ID'lerini kullanarak Yetkileri (Permissions) çek
        var permissions = await _context.RolePermissions
            .Where(rp => roleIds.Contains(rp.RoleId) && !rp.IsDeleted)
            .Select(rp => rp.Permission)
            .Distinct() 
            .ToListAsync();

        // 4. Token'ı 4 parametre ile üret (Yetkileri de gönderiyoruz)
        var token = await _jwtProvider.CreateTokenAsync(user.Id.ToString(), user.Email!, roles, permissions);

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


    public async Task<Result<string>> ChangePasswordAsync(ChangePasswordCommand request, CancellationToken cancellationToken)
    {
        var user = await _userManager.FindByIdAsync(request.UserId);
        if (user is null || user.IsDeleted)
            return Result<string>.Failure("Kullanıcı bulunamadı.");

        var result = await _userManager.ChangePasswordAsync(user, request.OldPassword, request.NewPassword);

        if (result.Succeeded)
        {
            return Result<string>.Success(user.Id.ToString(), "Şifre başarıyla değiştirildi.");
        }

        return Result<string>.Failure("Şifre değiştirilemedi: " +
            string.Join(", ", result.Errors.Select(e => e.Description)));
    }

    public async Task<string> LogoutAsync()
    {
        var token = _httpContextAccessor.HttpContext?.Request.Headers["Authorization"]
            .ToString().Replace("Bearer ", "");

        if (string.IsNullOrEmpty(token)) return "Token bulunamadı.";

        var handler = new JwtSecurityTokenHandler();
        var jwtToken = handler.ReadJwtToken(token);
        var expClaim = jwtToken.Claims.First(c => c.Type == JwtRegisteredClaimNames.Exp).Value;
        var expiryDate = DateTimeOffset.FromUnixTimeSeconds(long.Parse(expClaim)).UtcDateTime;

        var invalidToken = new InvalidToken
        {
            Token = token,
            ExpiryDate = expiryDate
        };

        await _context.InvalidTokens.AddAsync(invalidToken);

        await _unitOfWork.SaveChangesAsync();

        return "Başarıyla çıkış yapıldı.";
    }

    public async Task<Result<List<string>>> GetMyPermissionsAsync(string userId, CancellationToken cancellationToken)
    {
        // 1. Kullanıcıyı bul
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null || user.IsDeleted)
        {
            return Result<List<string>>.Failure("Kullanıcı bulunamadı.");
        }

        // 2. Kullanıcının rollerini al
        var userRoleNames = await _userManager.GetRolesAsync(user);
        if (!userRoleNames.Any())
        {
            return Result<List<string>>.Success(new List<string>(),"");
        }

        var roleIds = await _context.Roles
            .Where(r => userRoleNames.Contains(r.Name!))
            .Select(r => r.Id)
            .ToListAsync(cancellationToken);

        if (!roleIds.Any())
        {
            return Result<List<string>>.Success(new List<string>(),"");
        }

        var permissions = await _context.RolePermissions
            .Where(rp => roleIds.Contains(rp.RoleId) && !rp.IsDeleted)
            .Select(rp => rp.Permission)
            .Distinct()
            .ToListAsync(cancellationToken);

        return Result<List<string>>.Success(permissions,"");
    }
}
