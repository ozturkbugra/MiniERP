using MediatR;
using MiniERP.Domain.Common;

namespace MiniERP.Application.Features.Roles.Commands.AssignPermissions
{
    public sealed record AssignPermissionsToRoleCommand(
    Guid RoleId,
    List<string> Permissions) : IRequest<Result<string>>;
}
