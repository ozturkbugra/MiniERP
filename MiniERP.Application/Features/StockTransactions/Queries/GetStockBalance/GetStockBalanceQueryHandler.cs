using MediatR;
using MiniERP.Application.Interfaces;
using MiniERP.Domain.Common;
using MiniERP.Domain.Entities;
using MiniERP.Domain.Enums;

namespace MiniERP.Application.Features.StockTransactions.Queries.GetStockBalance
{
    public sealed class GetStockBalanceQueryHandler: IRequestHandler<GetStockBalanceQuery, Result<List<StockBalanceResponse>>>
    {
        private readonly IRepository<StockTransaction> _stockRepository;

        public GetStockBalanceQueryHandler(IRepository<StockTransaction> stockRepository)
        {
            _stockRepository = stockRepository;
        }

        public async Task<Result<List<StockBalanceResponse>>> Handle(GetStockBalanceQuery request,CancellationToken cancellationToken)
        {
            var transactions = await _stockRepository.GetAllAsync(
                cancellationToken,
                x => x.Product!,
                x => x.Warehouse!
            );

            var query = transactions.Where(x => !x.IsDeleted);

            if (request.WarehouseId.HasValue)
                query = query.Where(x => x.WarehouseId == request.WarehouseId);

            if (request.ProductId.HasValue)
                query = query.Where(x => x.ProductId == request.ProductId);

            var result = query
                .GroupBy(x => new {
                    x.ProductId,
                    ProductName = x.Product?.Name ?? "Bilinmeyen Ürün",
                    x.WarehouseId,
                    WarehouseName = x.Warehouse?.Name ?? "Bilinmeyen Depo"
                })
                .Select(g => new StockBalanceResponse(
                    g.Key.ProductId,
                    g.Key.ProductName,
                    g.Key.WarehouseId,
                    g.Key.WarehouseName,
                    g.Sum(s => s.Type == StockTransactionType.In ? s.Quantity : -s.Quantity)
                ))
                .ToList();

            return Result<List<StockBalanceResponse>>.Success(result,"İstek başarılı");
        }
    }
}
