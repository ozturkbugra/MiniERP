using MediatR;
using MiniERP.Domain.Common;

namespace MiniERP.Application.Features.Units.Queries.GetAllUnits
{
    public sealed record GetAllUnitsQuery() : IRequest<Result<List<GetAllUnitsResponse>>>;
}
