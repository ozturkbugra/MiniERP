using MediatR;
using MiniERP.Domain.Common;

namespace MiniERP.Application.Features.Warehouses.Queries.GetAllWarehouses
{
    public sealed record GetAllWarehousesQuery() : IRequest<Result<List<GetAllWarehousesResponse>>>;
}
