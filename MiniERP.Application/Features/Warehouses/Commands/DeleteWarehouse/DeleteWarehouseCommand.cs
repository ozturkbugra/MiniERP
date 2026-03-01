using MediatR;
using MiniERP.Domain.Common;

namespace MiniERP.Application.Features.Warehouses.Commands.DeleteWarehouse
{
    public sealed record DeleteWarehouseCommand(Guid Id) : IRequest<Result<string>>;
}
