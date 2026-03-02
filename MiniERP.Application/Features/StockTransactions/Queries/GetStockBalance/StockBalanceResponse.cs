namespace MiniERP.Application.Features.StockTransactions.Queries.GetStockBalance
{
    public sealed record StockBalanceResponse(
    Guid ProductId,
    string ProductName,
    Guid WarehouseId,
    string WarehouseName,
    decimal TotalQuantity
);
}
