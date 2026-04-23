using MediatR;
using MiniERP.Application.Interfaces;
using MiniERP.Domain.Common;
using MiniERP.Domain.Entities;
using MiniERP.Domain.Enums;

namespace MiniERP.Application.Features.Dashboard.Queries.GetStockSnapshot
{
    public sealed class GetStockSnapshotQueryHandler : IRequestHandler<GetStockSnapshotQuery, Result<List<StockSnapshotResponse>>>
    {
        private readonly IRepository<StockTransaction> _stockRepository;

        public GetStockSnapshotQueryHandler(IRepository<StockTransaction> stockRepository)
        {
            _stockRepository = stockRepository;
        }

        public async Task<Result<List<StockSnapshotResponse>>> Handle(GetStockSnapshotQuery request, CancellationToken cancellationToken)
        {
            // 🚀 Filtreye WarehouseId kontrolü eklendi
            var stockMoves = await _stockRepository.GetAllAsync(
                x => x.TransactionDate.Date <= request.TargetDate.Date &&
                     !x.IsDeleted &&
                     (!request.WarehouseId.HasValue || x.WarehouseId == request.WarehouseId.Value),
                cancellationToken,
                x => x.Product
            );

            var snapshot = stockMoves
                .GroupBy(x => new { x.ProductId, ProductName = x.Product?.Name ?? "Bilinmeyen Ürün" })
                .Select(g =>
                {
                    decimal quantity = g.Sum(x =>
                        ((int)x.Type == (int)StockTransactionType.In || (int)x.Type == 3)
                        ? x.Quantity
                        : -x.Quantity);

                    decimal value = g.Sum(x =>
                        ((int)x.Type == (int)StockTransactionType.In || (int)x.Type == 3)
                        ? (x.Quantity * x.UnitPrice)
                        : -(x.Quantity * x.UnitPrice));

                    return new StockSnapshotResponse(g.Key.ProductName, quantity, value);
                })
                .Where(x => x.RemainingQuantity != 0)
                .OrderByDescending(x => x.TotalValue)
                .ToList();

            return Result<List<StockSnapshotResponse>>.Success(snapshot, "Stok durumu depoya göre başarıyla filtrelendi.");
        }
    }
}