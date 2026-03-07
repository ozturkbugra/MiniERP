using MediatR;
using MiniERP.Domain.Common;

namespace MiniERP.Application.Features.Orders.Queries.GetOrderById
{
    public sealed record GetOrderByIdQuery(Guid Id) : IRequest<Result<GetOrderByIdResponse>>;
}
