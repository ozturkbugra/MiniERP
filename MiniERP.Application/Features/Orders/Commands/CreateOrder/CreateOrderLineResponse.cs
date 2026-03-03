namespace MiniERP.Application.Features.Orders.Commands.CreateOrder
{
    public sealed record CreateOrderLineResponse(
     Guid ProductId,
     Guid WarehouseId,
     decimal Quantity,
     decimal UnitPrice);
}
