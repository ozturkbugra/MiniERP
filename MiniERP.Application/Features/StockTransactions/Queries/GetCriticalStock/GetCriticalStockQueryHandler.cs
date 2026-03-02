using MediatR;
using MiniERP.Application.Interfaces;
using MiniERP.Domain.Common;
using MiniERP.Domain.Entities;
using MiniERP.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniERP.Application.Features.StockTransactions.Queries.GetCriticalStock
{
    public sealed class GetCriticalStockQueryHandler: IRequestHandler<GetCriticalStockQuery, Result<List<CriticalStockResponse>>>
    {
        private readonly IRepository<StockTransaction> _stockRepository;

        public GetCriticalStockQueryHandler(IRepository<StockTransaction> stockRepository)
        {
            _stockRepository = stockRepository;
        }

        public async Task<Result<List<CriticalStockResponse>>> Handle(GetCriticalStockQuery request,CancellationToken cancellationToken)
        {
            var transactions = await _stockRepository.GetAllAsync(
                cancellationToken,
                x => x.Product!,
                x => x.Warehouse!
            );

            var result = transactions
                .Where(x => !x.IsDeleted)
                .Where(x => !request.WarehouseId.HasValue || x.WarehouseId == request.WarehouseId)
                .GroupBy(x => new {
                    x.ProductId,
                    ProductName = x.Product?.Name ?? "Bilinmeyen Ürün",
                    CriticalLevel = x.Product?.CriticalStockLevel ?? 0,
                    x.WarehouseId,
                    WarehouseName = x.Warehouse?.Name ?? "Bilinmeyen Depo"
                })
                .Select(g => new {
                    g.Key.ProductId,
                    g.Key.ProductName,
                    g.Key.WarehouseId,
                    g.Key.WarehouseName,
                    g.Key.CriticalLevel,
                    Total = g.Sum(s => s.Type == StockTransactionType.In ? s.Quantity : -s.Quantity)
                })
                .Where(x => x.Total <= x.CriticalLevel)
                .Select(x => new CriticalStockResponse(
                    x.ProductId,
                    x.ProductName,
                    x.WarehouseId,
                    x.WarehouseName,
                    x.Total,
                    x.CriticalLevel
                ))
                .ToList();

            string message = result.Any() ? $"{result.Count} adet ürün kritik stok seviyesinde!" : "Şu an kritik seviyede ürün bulunmuyor, stoklar güvende.";

            return Result<List<CriticalStockResponse>>.Success(result, message);
        }
    }
}
