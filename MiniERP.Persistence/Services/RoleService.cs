using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MiniERP.Application.Features.Roles.Commands.UpdateRoles;
using MiniERP.Application.Features.Roles.Queries.GetAllRoles;
using MiniERP.Application.Features.Roles.Queries.GetRoleById;
using MiniERP.Application.Interfaces;
using MiniERP.Domain.Common;
using MiniERP.Persistence.IdentityModels;

namespace MiniERP.Persistence.Services;

public sealed class RoleService : IRoleService
{
    private readonly RoleManager<ApplicationRole> _roleManager;

    public RoleService(RoleManager<ApplicationRole> userManager)
    {
        _roleManager = userManager;
    }

    public async Task<Result> CreateRoleAsync(string name, string description)
    {
        var role = new ApplicationRole
        {
            Id = Guid.NewGuid(),
            Name = name,
            Description = description,

        };

        var result = await _roleManager.CreateAsync(role);

        if (result.Succeeded)
        {
            return Result.Success("Rol kaydı başarıyla tamamlandı");

        }

        var errored = result.Errors.Select(e => e.Description);
        return Result.Failure("Rol eklerken bir hata oluştu.", errored);
    }

    public async Task<Result<List<GetAllRolesQueryResponse>>> GetAllRolesAsync(CancellationToken cancellationToken)
    {
        var roles = await _roleManager.Roles
            .Where(r => !r.IsDeleted)
            .Select(r => new GetAllRolesQueryResponse(
                r.Id.ToString(),
                r.Name!,
                r.Description
            ))
            .ToListAsync(cancellationToken);

        return Result<List<GetAllRolesQueryResponse>>.Success(roles, "Rol listesi başarıyla getirildi.");
    }

    public async Task<Result<GetRoleByIdQueryResponse>> GetRoleByIdAsync(string id, CancellationToken cancellationToken)
    {
        var role = await _roleManager.FindByIdAsync(id);

        if (role is null || role.IsDeleted)
            return Result<GetRoleByIdQueryResponse>.Failure("Rol bulunamadı.");

        var response = new GetRoleByIdQueryResponse(role.Id.ToString(), role.Name!, role.Description);

        return Result<GetRoleByIdQueryResponse>.Success(response, "Rol bilgileri başarıyla getirildi.");
    }

    public async Task<Result<string>> UpdateRoleAsync(UpdateRoleCommand request, CancellationToken cancellationToken)
    {
        var role = await _roleManager.FindByIdAsync(request.Id);

        if (role is null || role.IsDeleted)
            return Result<string>.Failure("Güncellenecek rol bulunamadı.");

        role.Name = request.Name;
        role.Description = request.Description;

        var result = await _roleManager.UpdateAsync(role);

        if (result.Succeeded)
        {
            return Result<string>.Success(role.Id.ToString(), "Rol başarıyla güncellendi.");
        }

        return Result<string>.Failure(string.Join(", ", result.Errors.Select(e => e.Description)));
    }

    public async Task<Result<string>> DeleteRoleAsync(string id, CancellationToken cancellationToken)
    {
        var role = await _roleManager.FindByIdAsync(id);

        if (role is null || role.IsDeleted)
            return Result<string>.Failure("Silinecek rol bulunamadı.");

        role.IsDeleted = true;

        var result = await _roleManager.UpdateAsync(role);

        if (result.Succeeded)
        {
            return Result<string>.Success(role.Id.ToString(), "Rol başarıyla pasife çekildi (Soft Delete).");
        }

        return Result<string>.Failure(string.Join(", ", result.Errors.Select(e => e.Description)));
    }

    public async Task<bool> IsRoleNameUniqueAsync(string name, string? currentRoleId = null, CancellationToken cancellationToken = default)
    {
        var role = await _roleManager.FindByNameAsync(name);

        if (role is null) return true;

        if (currentRoleId is null) return false;

        // Eğer rol varsa ve ID'si bizim elimizdekiyle aynıysa (kendi ismidir), unique kabul edilir.
        // Farklıysa başka bir rol bu ismi kapmış demektir.
        return role.Id.ToString() == currentRoleId;
    }
}