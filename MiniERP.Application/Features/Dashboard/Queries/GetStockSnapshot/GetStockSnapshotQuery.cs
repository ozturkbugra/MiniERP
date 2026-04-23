using MediatR;
using MiniERP.Domain.Common;

namespace MiniERP.Application.Features.Dashboard.Queries.GetStockSnapshot
{
    public sealed record GetStockSnapshotQuery(DateTime TargetDate, Guid? WarehouseId) : IRequest<Result<List<StockSnapshotResponse>>>;
}
