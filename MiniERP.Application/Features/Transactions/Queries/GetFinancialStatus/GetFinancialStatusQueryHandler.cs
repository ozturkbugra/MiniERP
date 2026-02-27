using MediatR;
using MiniERP.Application.Interfaces;
using MiniERP.Domain.Common;
using MiniERP.Domain.Entities;
using CashEntity = MiniERP.Domain.Entities.Cash; // Alias kullanımı


namespace MiniERP.Application.Features.Transactions.Queries.GetFinancialStatus
{
    public sealed class GetFinancialStatusQueryHandler : IRequestHandler<GetFinancialStatusQuery, Result<FinancialStatusResponse>>
    {
        private readonly IRepository<CashEntity> _cashRepository;
        private readonly IRepository<Bank> _bankRepository;
        private readonly IRepository<CashTransaction> _cashTransactionRepository;
        private readonly IRepository<BankTransaction> _bankTransactionRepository;

        public GetFinancialStatusQueryHandler(
            IRepository<CashEntity> cashRepository,
            IRepository<Bank> bankRepository,
            IRepository<CashTransaction> cashTransactionRepository,
            IRepository<BankTransaction> bankTransactionRepository)
        {
            _cashRepository = cashRepository;
            _bankRepository = bankRepository;
            _cashTransactionRepository = cashTransactionRepository;
            _bankTransactionRepository = bankTransactionRepository;
        }

        public async Task<Result<FinancialStatusResponse>> Handle(GetFinancialStatusQuery request, CancellationToken cancellationToken)
        {
            // 1. Kartları Getir (İsimler lazım)
            var cashes = await _cashRepository.GetAllAsync(cancellationToken);
            var banks = await _bankRepository.GetAllAsync(cancellationToken);

            // 2. Hareket Sorgularını Hazırla (İşlem silinmemiş olmalı)
            var cashTxQuery = _cashTransactionRepository.GetAll().Where(x => !x.IsDeleted);
            var bankTxQuery = _bankTransactionRepository.GetAll().Where(x => !x.IsDeleted);

            // 3. Repository Köprüsü ile Verileri Çek
            var allCashTxs = await _cashTransactionRepository.ToListAsync(cashTxQuery, cancellationToken);
            var allBankTxs = await _bankTransactionRepository.ToListAsync(bankTxQuery, cancellationToken);

            // 4. Kasa Bazlı Bakiyeleri Eşleştir
            var cashBalances = cashes.Select(c => new AccountStatusResponse(
                c.Id,
                c.Name,
                allCashTxs.Where(t => t.CashId == c.Id).Sum(t => t.Debit - t.Credit)
            )).OrderByDescending(x => x.Balance).ToList();

            // 5. Banka Bazlı Bakiyeleri Eşleştir
            var bankBalances = banks.Select(b => new AccountStatusResponse(
                b.Id,
                b.BankName,
                allBankTxs.Where(t => t.BankId == b.Id).Sum(t => t.Debit - t.Credit)
            )).OrderByDescending(x => x.Balance).ToList();

            return Result<FinancialStatusResponse>.Success(
                new FinancialStatusResponse(cashBalances, bankBalances),
                "Tüm kasa ve banka bakiyeleri başarıyla listelendi."
            );
        }
    }
}
