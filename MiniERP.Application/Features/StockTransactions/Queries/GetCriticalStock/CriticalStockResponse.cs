namespace MiniERP.Application.Features.StockTransactions.Queries.GetCriticalStock
{
    public sealed record CriticalStockResponse(
    Guid ProductId,
    string ProductName,
    Guid WarehouseId,
    string WarehouseName,
    decimal CurrentQuantity,
    decimal CriticalLevel // Ürün kartındaki o meşhur eşik
);
}
