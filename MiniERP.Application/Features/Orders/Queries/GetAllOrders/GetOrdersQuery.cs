using MediatR;
using MiniERP.Domain.Common;

namespace MiniERP.Application.Features.Orders.Queries.GetAllOrders
{
    public sealed record GetOrdersQuery() : IRequest<Result<List<GetOrdersResponse>>>;
}
