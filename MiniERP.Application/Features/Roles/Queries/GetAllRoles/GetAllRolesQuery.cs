using MediatR;
using MiniERP.Domain.Common;

namespace MiniERP.Application.Features.Roles.Queries.GetAllRoles
{
    public sealed record GetAllRolesQuery()
    : IRequest<Result<List<GetAllRolesQueryResponse>>>;


}
