using MediatR;
using MiniERP.Domain.Common;

namespace MiniERP.Application.Features.Units.Queries.GetUnitById
{
    public sealed record GetUnitByIdQuery(Guid Id) : IRequest<Result<GetUnitByIdResponse>>;
}
