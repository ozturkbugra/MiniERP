using MediatR;
using MiniERP.Application.Interfaces;
using MiniERP.Domain.Common;
using MiniERP.Domain.Entities;
using MiniERP.Domain.Enums;

namespace MiniERP.Application.Features.Dashboard.Queries.GetDashboardSummary
{
    public sealed class GetDashboardSummaryQueryHandler : IRequestHandler<GetDashboardSummaryQuery, Result<GetDashboardSummaryResponse>>
    {
        private readonly IRepository<Invoice> _invoiceRepository;
        private readonly IRepository<StockTransaction> _stockRepository;
        private readonly IRepository<CashTransaction> _cashRepository;
        private readonly IRepository<BankTransaction> _bankRepository;

        public GetDashboardSummaryQueryHandler(
            IRepository<Invoice> invoiceRepository,
            IRepository<StockTransaction> stockRepository,
            IRepository<CashTransaction> cashRepository,
            IRepository<BankTransaction> bankRepository)
        {
            _invoiceRepository = invoiceRepository;
            _stockRepository = stockRepository;
            _cashRepository = cashRepository;
            _bankRepository = bankRepository;
        }

        public async Task<Result<GetDashboardSummaryResponse>> Handle(GetDashboardSummaryQuery request, CancellationToken cancellationToken)
        {
            try
            {
                // 1. TOPLAM CİRO (Sadece Onaylı Satış Faturaları)
                var allInvoices = await _invoiceRepository.GetAllAsync(cancellationToken);
                var approvedSales = allInvoices.Where(x => x.Type == InvoiceType.Sales && x.Status == InvoiceStatus.Approved).ToList();
                var totalSales = approvedSales.Sum(x => x.GrandTotal);

                // 2. KASA & BANKA BAKİYESİ (Giren Para - Çıkan Para)
                var cashes = await _cashRepository.GetAllAsync(cancellationToken);
                var banks = await _bankRepository.GetAllAsync(cancellationToken);

                var totalCash = cashes.Sum(x => x.Debit - x.Credit);
                var totalBank = banks.Sum(x => x.Debit - x.Credit);
                var totalCashBankBalance = totalCash + totalBank;

                // 3. ANLIK STOK DEĞERİ (Miktar * Fiyat)
                var stockMoves = await _stockRepository.GetAllAsync(cancellationToken);
                var totalStockValue = stockMoves.Sum(x =>
                    ((int)x.Type == (int)StockTransactionType.In || (int)x.Type == 3)
                        ? (x.Quantity * x.UnitPrice)
                        : -(x.Quantity * x.UnitPrice));

                // 4. AYLIK SATIŞ GRAFİĞİ (Son 6 Ay)
                var sixMonthsAgo = DateTime.Now.AddMonths(-5);
                var startDate = new DateTime(sixMonthsAgo.Year, sixMonthsAgo.Month, 1);

                var monthlySales = approvedSales
                    .Where(x => x.InvoiceDate >= startDate)
                    .GroupBy(x => new { x.InvoiceDate.Year, x.InvoiceDate.Month })
                    .OrderBy(g => g.Key.Year).ThenBy(g => g.Key.Month)
                    .Select(g => new MonthlySalesResponse(
                        Month: new DateTime(g.Key.Year, g.Key.Month, 1).ToString("MMM"), // "Oca", "Şub" formatı
                        Amount: g.Sum(x => x.GrandTotal)
                    )).ToList();

                // 5. SON İŞLEMLER (Timeline - Son 5 Fatura Hareketi)
                var recentActivities = allInvoices
                    .OrderByDescending(x => x.CreatedDate)
                    .Take(5)
                    .Select(x => new RecentActivityResponse(
                        Date: x.CreatedDate,
                        Description: $"{x.InvoiceNumber} nolu {GetInvoiceTypeName(x.Type)} kesildi.",
                        Type: "Invoice",
                        StatusColor: x.Status == InvoiceStatus.Approved ? "text-success" : (x.Status == InvoiceStatus.Canceled ? "text-danger" : "text-secondary")
                    )).ToList();

                // TÜM VERİYİ TEK PAKETTE DÖNÜYORUZ (record mimarisi ile)
                var response = new GetDashboardSummaryResponse
                {
                    TotalSales = totalSales,
                    TotalStockValue = totalStockValue,
                    TotalCashBankBalance = totalCashBankBalance,
                    MonthlySales = monthlySales,
                    RecentActivities = recentActivities
                };

                return Result<GetDashboardSummaryResponse>.Success(response,"Dashboard verisi başarıyla hesaplandı.");
            }
            catch (Exception ex)
            {
                return Result<GetDashboardSummaryResponse>.Failure($"Dashboard verisi hesaplanırken hata oluştu: {ex.Message}");
            }
        }

        // Timeline açıklamalarını güzelleştirmek için küçük bir yardımcı metod
        private string GetInvoiceTypeName(InvoiceType type) => type switch
        {
            InvoiceType.Sales => "Satış Faturası",
            InvoiceType.Purchase => "Alım Faturası",
            InvoiceType.SalesReturn => "Satış İade Faturası",
            InvoiceType.PurchaseReturn => "Alım İade Faturası",
            _ => "Fatura"
        };
    }
}