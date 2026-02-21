using Microsoft.AspNetCore.Identity;
using MiniERP.Application.Interfaces;
using MiniERP.Domain.Common;
using MiniERP.Persistence.IdentityModels;

namespace MiniERP.Persistence.Services;

public sealed class RoleService : IRoleService
{
    private readonly RoleManager<ApplicationRole> _userManager;

    public RoleService(RoleManager<ApplicationRole> userManager)
    {
        _userManager = userManager;
    }

    public async Task<Result> CreateRoleAsync(string name, string description)
    {
        var role = new ApplicationRole
        {
            Id = Guid.NewGuid(),
            Name = name,
            Description = description,

        };

        var result = await _userManager.CreateAsync(role);

        if (result.Succeeded)
        {
            return Result.Success("Rol kaydı başarıyla tamamlandı");

        }

        var errored = result.Errors.Select(e => e.Description);
        return Result.Failure("Rol eklerken bir hata oluştu.", errored);
    }
}