using MediatR;
using MiniERP.Domain.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniERP.Application.Features.Roles.Queries.GetPermissionsByRoleId
{
    public sealed record GetPermissionsByRoleIdQuery(Guid RoleId) : IRequest<Result<List<string>>>;
}
