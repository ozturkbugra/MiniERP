using MediatR;
using MiniERP.Domain.Common;

namespace MiniERP.Application.Features.Roles.Commands.UpdateRoles
{
    public sealed record UpdateRoleCommand(
    string Id,
    string Name,
    string Description) : IRequest<Result<string>>;

}
