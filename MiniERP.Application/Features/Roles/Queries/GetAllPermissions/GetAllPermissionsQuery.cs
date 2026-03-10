using MediatR;
using MiniERP.Domain.Common;

namespace MiniERP.Application.Features.Roles.Queries.GetAllPermissions
{
    public sealed record GetAllPermissionsQuery() : IRequest<Result<List<PermissionGroupResponse>>>;
}
