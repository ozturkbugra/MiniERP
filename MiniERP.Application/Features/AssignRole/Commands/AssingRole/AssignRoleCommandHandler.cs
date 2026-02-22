using MediatR;
using MiniERP.Application.Interfaces;
using MiniERP.Domain.Common;

namespace MiniERP.Application.Features.AssignRole.Commands.AssingRole;

public sealed class AssignRoleCommandHandler : IRequestHandler<AssignRoleCommand, Result>
{
    private readonly IAuthService _authService;

    public AssignRoleCommandHandler(IAuthService authService)
    {
        _authService = authService;
    }

    public async Task<Result> Handle(AssignRoleCommand request, CancellationToken cancellationToken)
    {
        // İşi doğrudan AuthService'e devrediyoruz
        return await _authService.AssignRoleAsync(request.UserId, request.RoleName);
    }
}