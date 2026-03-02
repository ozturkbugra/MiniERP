using MediatR;
using MiniERP.Domain.Common;

namespace MiniERP.Application.Features.StockTransactions.Queries.GetCriticalStock
{
    public sealed record GetCriticalStockQuery(Guid? WarehouseId) : IRequest<Result<List<CriticalStockResponse>>>;
}
