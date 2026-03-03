using MediatR;
using MiniERP.Domain.Common;

namespace MiniERP.Application.Features.Orders.Commands.ApproveOrder
{
    public sealed record ApproveOrderCommand(Guid OrderId) : IRequest<Result<string>>;
}
