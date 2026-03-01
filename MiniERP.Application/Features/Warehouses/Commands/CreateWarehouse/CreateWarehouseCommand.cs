using MediatR;
using MiniERP.Domain.Common;

namespace MiniERP.Application.Features.Warehouses.Commands.CreateWarehouse
{
    public sealed record CreateWarehouseCommand(string Code, string Name, string? Location) : IRequest<Result<string>>;
}
