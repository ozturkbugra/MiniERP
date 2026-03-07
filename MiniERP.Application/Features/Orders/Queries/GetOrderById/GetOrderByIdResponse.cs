namespace MiniERP.Application.Features.Orders.Queries.GetOrderById
{
    public sealed record GetOrderByIdResponse(
    Guid Id,
    string OrderNumber,
    DateTime OrderDate,
    string CustomerName,
    string WarehouseName,
    string Status,
    string CreatedByName,
    List<OrderLineResponse> Lines
);
}
