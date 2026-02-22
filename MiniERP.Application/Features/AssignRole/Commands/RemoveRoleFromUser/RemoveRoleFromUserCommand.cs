using MediatR;
using MiniERP.Domain.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniERP.Application.Features.AssignRole.Commands.RemoveRoleFromUser
{
    public sealed record RemoveRoleFromUserCommand(
    string UserId,
    string RoleName) : IRequest<Result<string>>;
}
