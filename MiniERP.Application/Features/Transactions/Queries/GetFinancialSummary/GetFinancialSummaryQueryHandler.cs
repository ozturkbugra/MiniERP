using MediatR;
using MiniERP.Application.Interfaces;
using MiniERP.Domain.Common;
using MiniERP.Domain.Entities;

namespace MiniERP.Application.Features.Transactions.Queries.GetFinancialSummary
{
    public sealed class GetFinancialSummaryQueryHandler : IRequestHandler<GetFinancialSummaryQuery, Result<FinancialSummaryResponse>>
    {
        private readonly IRepository<CashTransaction> _cashTransactionRepository;
        private readonly IRepository<BankTransaction> _bankTransactionRepository;

        public GetFinancialSummaryQueryHandler(
            IRepository<CashTransaction> cashTransactionRepository,
            IRepository<BankTransaction> bankTransactionRepository)
        {
            _cashTransactionRepository = cashTransactionRepository;
            _bankTransactionRepository = bankTransactionRepository;
        }

        public async Task<Result<FinancialSummaryResponse>> Handle(GetFinancialSummaryQuery request, CancellationToken cancellationToken)
        {
            // 1. Nakit İşlemleri Sorgusunu Hazırla (IQueryable)
            var cashQuery = _cashTransactionRepository.GetAll()
                .Where(x => !x.IsDeleted);

            // 2. Kendi Repository köprümüzü kullanarak asenkron listeye çevir
            var cashTransactions = await _cashTransactionRepository.ToListAsync(cashQuery, cancellationToken);
            var totalCash = cashTransactions.Sum(x => x.Debit - x.Credit);

            // 3. Banka İşlemleri Sorgusunu Hazırla (IQueryable)
            var bankQuery = _bankTransactionRepository.GetAll()
                .Where(x => !x.IsDeleted);

            // 4. Yine Repository köprüsü üzerinden veriyi çek
            var bankTransactions = await _bankTransactionRepository.ToListAsync(bankQuery, cancellationToken);
            var totalBank = bankTransactions.Sum(x => x.Debit - x.Credit);

            return Result<FinancialSummaryResponse>.Success(
                new FinancialSummaryResponse(totalCash, totalBank, totalCash + totalBank),
                "Finansal Özet Başarıyla Hazırlandı."
            );
        }
    }
}
