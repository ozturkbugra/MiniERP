namespace MiniERP.Application.Features.Warehouses.Queries.GetWarehouseById
{
    public sealed record GetWarehouseByIdResponse(Guid Id, string Code, string Name, string? Location);
}
