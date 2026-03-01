using MediatR;
using MiniERP.Domain.Common;

namespace MiniERP.Application.Features.StockTransactions.Queries.GetAllStockTransactions
{
    public sealed record GetAllStockTransactionsQuery() : IRequest<Result<List<GetAllStockTransactionsResponse>>>;
}
