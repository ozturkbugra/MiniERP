using FluentValidation;
using MiniERP.Application.Features.Roles.Commands.UpdateRoles;
using MiniERP.Application.Interfaces;

public sealed class UpdateRoleCommandValidator : AbstractValidator<UpdateRoleCommand>
{
    private readonly IRoleService _roleService;

    public UpdateRoleCommandValidator(IRoleService roleService)
    {
        _roleService = roleService;

        RuleFor(p => p.Name)
            .NotEmpty().WithMessage("Rol adı boş geçilemez.")
            .MustAsync(async (command, name, ct) =>
                await _roleService.IsRoleNameUniqueAsync(name, command.Id, ct))
            .WithMessage("Bu rol adı başka bir rol tarafından kullanılıyor!");
    }
}