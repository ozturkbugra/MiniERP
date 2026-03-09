using MediatR;
using MiniERP.Application.Interfaces;
using MiniERP.Domain.Common;
using MiniERP.Domain.Entities;
using MiniERP.Domain.Enums;

namespace MiniERP.Application.Features.Reports.Queries.GetDashboardSummary
{
    public sealed class GetDashboardSummaryQueryHandler : IRequestHandler<GetDashboardSummaryQuery, Result<DashboardSummaryResponse>>
    {
        private readonly IRepository<CustomerTransaction> _customerTransactionRepository;

        public GetDashboardSummaryQueryHandler(IRepository<CustomerTransaction> customerTransactionRepository)
        {
            _customerTransactionRepository = customerTransactionRepository;
        }

        public async Task<Result<DashboardSummaryResponse>> Handle(GetDashboardSummaryQuery request, CancellationToken cancellationToken)
        {
            var today = DateTime.UtcNow.Date;

            // BÜTÜN AKTİF CARİ HAREKETLERİ ÇEKİYORUZ
            var allActiveTransactions = await _customerTransactionRepository.GetAllAsync(
             x => !x.IsDeleted,
             cancellationToken, 
             x => x.Customer    
            );

            // 1. GÜNLÜK CİRO HESAPLAMA (Sadece bugünkü satış faturaları)
            decimal dailyTurnover = allActiveTransactions
                .Where(x => x.Type == TransactionType.SalesInvoice && x.Date.Date == today)
                .Sum(x => x.Debit);

            // 2. EN ÇOK BORCU OLAN 5 MÜŞTERİ (Müşteri Riski)
            var topDebtors = allActiveTransactions
                .GroupBy(x => new { x.CustomerId, x.Customer?.Name })
                .Select(g => new CustomerRiskResponse(
                    CustomerId: g.Key.CustomerId,
                    CustomerName: g.Key.Name ?? "Bilinmeyen Müşteri",
                    Balance: g.Sum(x => x.Debit) - g.Sum(x => x.Credit)
                ))
                .Where(x => x.Balance > 0)
                .OrderByDescending(x => x.Balance)
                .Take(5)
                .ToList();

            var result = new DashboardSummaryResponse(dailyTurnover, topDebtors);

            return Result<DashboardSummaryResponse>.Success(result, "Dashboard verileri başarıyla hesaplandı.");
        }
    }
}
