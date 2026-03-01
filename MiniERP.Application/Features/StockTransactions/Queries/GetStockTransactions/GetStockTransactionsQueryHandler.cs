using MediatR;
using MiniERP.Application.Features.StockTransactions.Queries.GetAllStockTransactions;
using MiniERP.Application.Interfaces;
using MiniERP.Domain.Common;
using MiniERP.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniERP.Application.Features.StockTransactions.Queries.GetStockTransactions
{
    public sealed class GetStockTransactionsQueryHandler : IRequestHandler<GetStockTransactionsQuery, Result<List<GetAllStockTransactionsResponse>>>
    {
        private readonly IRepository<StockTransaction> _transactionRepository;
        private readonly IAppUserService _appUserService;

        public GetStockTransactionsQueryHandler(IRepository<StockTransaction> transactionRepository, IAppUserService appUserService)
        {
            _transactionRepository = transactionRepository;
            _appUserService = appUserService;
        }


        public async Task<Result<List<GetAllStockTransactionsResponse>>> Handle(GetStockTransactionsQuery request, CancellationToken cancellationToken)
        {
           
            var query = await _transactionRepository.GetAllAsync(cancellationToken,
                x => x.Product,
                x => x.Warehouse,
                x => x.Customer);

            var transactions = query.AsQueryable();

            if (request.ProductId.HasValue)
                transactions = transactions.Where(x => x.ProductId == request.ProductId.Value);

            if (request.WarehouseId.HasValue)
                transactions = transactions.Where(x => x.WarehouseId == request.WarehouseId.Value);

            if (request.CustomerId.HasValue)
                transactions = transactions.Where(x => x.CustomerId == request.CustomerId.Value);

          
            var dataList = transactions.OrderByDescending(x => x.TransactionDate).ToList();

            var userIds = dataList.Select(x => x.CreatedBy).Where(id => !string.IsNullOrEmpty(id)).Distinct().ToList();
            var usersDictionary = await _appUserService.GetUserNamesByIdsAsync(userIds, cancellationToken);

            var response = dataList.Select(x =>
            {
                string createdName = "Sistem";
                if (!string.IsNullOrEmpty(x.CreatedBy))
                {
                    usersDictionary.TryGetValue(x.CreatedBy, out var name);
                    createdName = name ?? "Sistem";
                }

                return new GetAllStockTransactionsResponse(
                    x.Id,
                    x.DocumentNo,
                    x.TransactionDate,
                    x.Quantity,
                    x.UnitPrice,
                    x.Quantity * x.UnitPrice,
                    x.Product?.Name ?? "Tanımsız Ürün",
                    x.Warehouse?.Name ?? "Tanımsız Depo",
                    x.Customer?.Name ?? "Tanımsız Cari",
                    createdName,
                    x.Type.ToString() == "In" ? "Giriş" : "Çıkış",
                    x.Description ?? ""
                );
            }).ToList();

            return Result<List<GetAllStockTransactionsResponse>>.Success(response, "İşlem başarıyla tamamlandı.");
        }

    }
}
