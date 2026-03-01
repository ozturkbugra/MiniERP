namespace MiniERP.Application.Features.StockTransactions.Queries.GetAllStockTransactions
{
    public sealed record GetAllStockTransactionsResponse(
    Guid Id,
    string DocumentNo,
    DateTime TransactionDate,
    decimal Quantity,
    decimal UnitPrice,
    decimal TotalPrice,
    string ProductName,
    string WarehouseName,
    string CustomerName,
    string CreatedBy, // İşlemi yapan kullanıcı
    string TypeName,
    string Description);
}
