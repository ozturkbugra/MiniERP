using MediatR;
using MiniERP.Domain.Common;

namespace MiniERP.Application.Features.Roles.Commands.DeleteRoles
{
    public sealed record DeleteRoleCommand(string Id) : IRequest<Result<string>>;
}
