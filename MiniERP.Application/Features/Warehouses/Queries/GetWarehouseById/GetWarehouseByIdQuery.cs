using MediatR;
using MiniERP.Domain.Common;

namespace MiniERP.Application.Features.Warehouses.Queries.GetWarehouseById
{
    public sealed record GetWarehouseByIdQuery(Guid Id) : IRequest<Result<GetWarehouseByIdResponse>>;
}
