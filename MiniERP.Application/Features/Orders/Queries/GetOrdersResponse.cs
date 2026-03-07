namespace MiniERP.Application.Features.Orders.Queries
{
    public sealed record GetOrdersResponse(
     Guid Id,
     string OrderNumber,
     DateTime OrderDate,
     Guid CustomerId,
     string CustomerName,
     string Status,
     Guid WarehouseId,
     string WarehouseName,
     string CreatedByName,
     string? UpdatedByName
 );
}
