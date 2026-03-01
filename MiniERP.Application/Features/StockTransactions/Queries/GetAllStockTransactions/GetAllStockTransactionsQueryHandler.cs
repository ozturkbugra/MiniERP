using AutoMapper;
using MediatR;
using MiniERP.Application.Interfaces;
using MiniERP.Domain.Common;
using MiniERP.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniERP.Application.Features.StockTransactions.Queries.GetAllStockTransactions
{
    public sealed class GetAllStockTransactionsQueryHandler : IRequestHandler<GetAllStockTransactionsQuery, Result<List<GetAllStockTransactionsResponse>>>
    {
        private readonly IRepository<StockTransaction> _transactionRepository;
        private readonly IAppUserService _appUserService;

        public GetAllStockTransactionsQueryHandler(IRepository<StockTransaction> transactionRepository, IAppUserService appUserService)
        {
            _transactionRepository = transactionRepository;
            _appUserService = appUserService;
        }

        public async Task<Result<List<GetAllStockTransactionsResponse>>> Handle(GetAllStockTransactionsQuery request, CancellationToken cancellationToken)
        {
            // 1. Veriyi ilişkileriyle (Ürün, Depo, Cari) çek
            var transactions = await _transactionRepository.GetAllAsync(cancellationToken,
                x => x.Product,
                x => x.Warehouse,
                x => x.Customer);

            // 2. Kullanıcı ID'lerini topla (CreatedBy)
            var userIds = transactions
                .Select(x => x.CreatedBy)
                .Where(id => !string.IsNullOrEmpty(id))
                .Distinct()
                .ToList();

            // 3. İsimleri Dictionary olarak al (Senin IAppUserService içindeki o meşhur mermi)
            var usersDictionary = await _appUserService.GetUserNamesByIdsAsync(userIds, cancellationToken);

            // 4. MANUEL BİRLEŞTİRME (Kasa modülündeki o jilet standart)
            var response = transactions.OrderByDescending(x => x.TransactionDate).Select(x =>
            {
                // CreatedBy eşleştirmesi
                string createdName = "Sistem";
                if (!string.IsNullOrEmpty(x.CreatedBy))
                {
                    usersDictionary.TryGetValue(x.CreatedBy, out var name);
                    createdName = name ?? "Sistem";
                }

                return new GetAllStockTransactionsResponse(
                    Id: x.Id,
                    DocumentNo: x.DocumentNo,
                    TransactionDate: x.TransactionDate,
                    Quantity: x.Quantity,
                    UnitPrice: x.UnitPrice,
                    TotalPrice: x.Quantity * x.UnitPrice,
                    ProductName: x.Product?.Name ?? "Tanımsız Ürün",
                    WarehouseName: x.Warehouse?.Name ?? "Tanımsız Depo",
                    CustomerName: x.Customer?.Name ?? "Tanımsız Cari",
                    CreatedBy: createdName,
                    TypeName: x.Type.ToString() == "In" ? "Giriş" : "Çıkış",
                    Description: x.Description ?? ""
                );
            }).ToList();

            return Result<List<GetAllStockTransactionsResponse>>.Success(response, "Stok hareketleri başarıyla listelendi.");
        }
    }
}
