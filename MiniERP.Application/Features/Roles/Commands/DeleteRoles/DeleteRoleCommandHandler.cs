using MediatR;
using MiniERP.Application.Interfaces;
using MiniERP.Domain.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniERP.Application.Features.Roles.Commands.DeleteRoles
{
    public sealed class DeleteRoleCommandHandler : IRequestHandler<DeleteRoleCommand, Result<string>>
    {
        private readonly IRoleService _roleService;

        public DeleteRoleCommandHandler(IRoleService roleService)
        {
            _roleService = roleService;
        }

        public async Task<Result<string>> Handle(DeleteRoleCommand request, CancellationToken cancellationToken)
        {
            return await _roleService.DeleteRoleAsync(request.Id, cancellationToken);
        }
    }
}
