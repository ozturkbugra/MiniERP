using MediatR;
using MiniERP.Domain.Common;
using MiniERP.Domain.Enums;

namespace MiniERP.Application.Features.Orders.Commands.CreateOrder
{
    public sealed record CreateOrderCommand(
    string OrderNumber,
    DateTime OrderDate,
    string? Description,
    OrderType Type,
    Guid CustomerId,
    List<CreateOrderLineResponse> OrderLines) : IRequest<Result<string>>;
}
