using MediatR;
using MiniERP.Application.Interfaces;
using MiniERP.Domain.Common;

namespace MiniERP.Application.Features.AssignRole.Commands.RemoveRoleFromUser
{
    public sealed class RemoveRoleFromUserCommandHandler : IRequestHandler<RemoveRoleFromUserCommand, Result<string>>
    {
        private readonly IAuthService _authService;

        public RemoveRoleFromUserCommandHandler(IAuthService authService) => _authService = authService;

        public async Task<Result<string>> Handle(RemoveRoleFromUserCommand request, CancellationToken cancellationToken)
        {
            return await _authService.RemoveRoleFromUserAsync(request.UserId, request.RoleName);
        }
    }
}
