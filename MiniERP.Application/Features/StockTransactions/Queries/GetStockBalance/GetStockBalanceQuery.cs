using MediatR;
using MiniERP.Domain.Common;

namespace MiniERP.Application.Features.StockTransactions.Queries.GetStockBalance
{
    public sealed record GetStockBalanceQuery(
    Guid? WarehouseId,
    Guid? ProductId
) : IRequest<Result<List<StockBalanceResponse>>>;
}
