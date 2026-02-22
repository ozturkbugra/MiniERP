using MediatR;
using MiniERP.Domain.Common;

namespace MiniERP.Application.Features.Roles.Queries.GetRoleById
{
    public sealed record GetRoleByIdQuery(string Id) : IRequest<Result<GetRoleByIdQueryResponse>>;
}
