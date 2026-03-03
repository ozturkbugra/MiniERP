using MediatR;
using MiniERP.Domain.Common;

namespace MiniERP.Application.Features.Orders.Commands.CancelOrder
{
    public sealed record CancelOrderCommand(Guid OrderId) : IRequest<Result<string>>;
}
