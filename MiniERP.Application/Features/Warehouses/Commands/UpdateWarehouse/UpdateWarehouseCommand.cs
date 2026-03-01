using MediatR;
using MiniERP.Domain.Common;

namespace MiniERP.Application.Features.Warehouses.Commands.UpdateWarehouse
{
    public sealed record UpdateWarehouseCommand(Guid Id, string Code, string Name, string? Location) : IRequest<Result<string>>;
}
