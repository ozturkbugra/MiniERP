using MediatR;
using MiniERP.Application.Interfaces;
using MiniERP.Domain.Common;
using MiniERP.Domain.Entities;
using MiniERP.Domain.Enums;

namespace MiniERP.Application.Features.Products.Queries.GetProductLedger
{
    public sealed class GetProductLedgerQueryHandler : IRequestHandler<GetProductLedgerQuery, Result<List<ProductLedgerResponse>>>
    {
        private readonly IRepository<StockTransaction> _stockRepository;

        public GetProductLedgerQueryHandler(IRepository<StockTransaction> stockRepository)
        {
            _stockRepository = stockRepository;
        }

        public async Task<Result<List<ProductLedgerResponse>>> Handle(GetProductLedgerQuery request, CancellationToken cancellationToken)
        {
            var transactions = await _stockRepository.GetAllAsync(
                x => x.ProductId == request.ProductId && !x.IsDeleted,
                cancellationToken,
                x => x.Customer
            );

            // 🚀 1. Önce tarih sırasına göre diziyoruz (Yürüyen bakiye hesaplamak için kronolojik olmalı)
            var sortedTransactions = transactions.OrderBy(x => x.TransactionDate).ThenBy(x => x.CreatedDate).ToList();

            var responseList = new List<ProductLedgerResponse>();

            decimal currentQuantity = 0;
            decimal currentValue = 0;

            // 🚀 2. Üzerine ekleye ekleye (Kümülatif) satırları oluşturuyoruz
            foreach (var t in sortedTransactions)
            {
                // Yön bulma: Type In (1) veya Açılış (3) ise GİRİŞ, aksi halde ÇIKIŞ
                bool isIn = (int)t.Type == (int)StockTransactionType.In || (int)t.Type == 3;

                // Matematiksel yönler
                decimal signedQuantity = isIn ? t.Quantity : -t.Quantity;
                decimal lineTotal = t.Quantity * t.UnitPrice;
                decimal signedLineTotal = isIn ? lineTotal : -lineTotal;

                // Bakiyeleri güncelliyoruz
                currentQuantity += signedQuantity;
                currentValue += signedLineTotal;

                responseList.Add(new ProductLedgerResponse(
                    Date: t.TransactionDate,
                    FirmName: t.Customer?.Name ?? "Açılış/Devir İşlemi",
                    TransactionType: t.Description ?? (isIn ? "Giriş" : "Çıkış"),
                    Quantity: signedQuantity, // + veya - olarak gösterelim ki giren çıkan belli olsun
                    UnitPrice: t.UnitPrice,
                    LineTotal: lineTotal, // Bu işlemin kendi hacmi
                    RunningQuantity: currentQuantity, // 🚀 Güncel Depo Bakiyesi
                    RunningValue: currentValue,       // 🚀 Güncel Stok Değeri
                    DocumentNo: t.DocumentNo
                ));
            }

            // 🚀 3. Kullanıcı en son işlemi en üstte görsün diye listeyi tersine çeviriyoruz
            var finalResponse = responseList.OrderByDescending(x => x.Date).ToList();

            return Result<List<ProductLedgerResponse>>.Success(finalResponse, "Ürün defteri başarıyla oluşturuldu.");
        }
    }
}