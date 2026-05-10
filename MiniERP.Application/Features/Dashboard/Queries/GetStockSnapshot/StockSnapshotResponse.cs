namespace MiniERP.Application.Features.Dashboard.Queries.GetStockSnapshot
{
    public sealed record StockSnapshotResponse(
         string ProductName,
         decimal RemainingQuantity,
         decimal TotalValue      
     );
}
