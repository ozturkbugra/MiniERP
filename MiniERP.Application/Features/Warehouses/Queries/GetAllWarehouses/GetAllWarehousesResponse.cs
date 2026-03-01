namespace MiniERP.Application.Features.Warehouses.Queries.GetAllWarehouses
{
    public sealed record GetAllWarehousesResponse(Guid Id, string Code, string Name, string? Location);
}
