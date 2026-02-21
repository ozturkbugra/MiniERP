using MediatR;
using MiniERP.Application.Features.Roles.Commands;
using MiniERP.Application.Interfaces;
using MiniERP.Domain.Common;

namespace MiniERP.Application.Features.Roles.Commands.CreateUser;

public sealed class CreateRoleCommandHandler : IRequestHandler<CreateRoleCommand, Result>
{
    private readonly IRoleService _roleService;

    public CreateRoleCommandHandler(IRoleService roleService)
    {
        _roleService = roleService;
    }
    public async Task<Result> Handle(CreateRoleCommand request, CancellationToken cancellationToken)
    {
        var response = await _roleService.CreateRoleAsync(request.Name, request.Description);
        return response;
    }
}