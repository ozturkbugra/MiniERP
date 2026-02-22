using FluentValidation;
using MiniERP.Application.Features.Roles.Commands.CreateRoles;
using MiniERP.Application.Interfaces;

public sealed class CreateRoleCommandValidator : AbstractValidator<CreateRoleCommand>
{
    private readonly IRoleService _roleService;

    public CreateRoleCommandValidator(IRoleService roleService)
    {
        _roleService = roleService;

        RuleFor(p => p.Name)
            .NotEmpty().WithMessage("Rol adı boş geçilemez.")
            .MustAsync(async (name, ct) => await _roleService.IsRoleNameUniqueAsync(name, null, ct))
            .WithMessage("Bu rol adı zaten kullanılıyor!");
    }
}