using MediatR;
using MiniERP.Application.Interfaces;
using MiniERP.Domain.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniERP.Application.Features.Roles.Commands.UpdateRoles
{
    public sealed class UpdateRoleCommandHandler : IRequestHandler<UpdateRoleCommand, Result<string>>
    {
        private readonly IRoleService _roleService;

        public UpdateRoleCommandHandler(IRoleService roleService)
        {
            _roleService = roleService;
        }

        public async Task<Result<string>> Handle(UpdateRoleCommand request, CancellationToken cancellationToken)
        {
            return await _roleService.UpdateRoleAsync(request, cancellationToken);
        }
    }
}
