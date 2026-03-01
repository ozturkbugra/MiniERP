using MediatR;
using MiniERP.Application.Features.StockTransactions.Queries.GetAllStockTransactions;
using MiniERP.Domain.Common;

namespace MiniERP.Application.Features.StockTransactions.Queries.GetStockTransactions
{
    public sealed record GetStockTransactionsQuery(
    Guid? ProductId = null,
    Guid? WarehouseId = null,
    Guid? CustomerId = null) : IRequest<Result<List<GetAllStockTransactionsResponse>>>;
}
