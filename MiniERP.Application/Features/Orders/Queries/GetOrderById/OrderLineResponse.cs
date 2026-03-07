namespace MiniERP.Application.Features.Orders.Queries.GetOrderById
{
    public sealed record OrderLineResponse(
     Guid ProductId,
     string ProductName,
     decimal Quantity,
     decimal UnitPrice,
     string WarehouseName
 );
}
