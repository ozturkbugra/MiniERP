using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
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

        if (role is null)
            return Result<GetRoleByIdQueryResponse>.Failure("Rol bulunamadı.");

        var response = new GetRoleByIdQueryResponse(role.Id.ToString(), role.Name!, role.Description);

        return Result<GetRoleByIdQueryResponse>.Success(response, "Rol bilgileri başarıyla getirildi.");
    }
}