using MediatR;
using MiniERP.Application.Interfaces;
using MiniERP.Domain.Common;
using MiniERP.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniERP.Application.Features.Transactions.Queries.GetAccountBalance
{
    public sealed class GetAccountBalanceQueryHandler : IRequestHandler<GetAccountBalanceQuery, Result<AccountBalanceResponse>>
    {
        private readonly IRepository<CashTransaction> _cashTransactionRepository;
        private readonly IRepository<BankTransaction> _bankTransactionRepository;

        public GetAccountBalanceQueryHandler(
            IRepository<CashTransaction> cashTransactionRepository,
            IRepository<BankTransaction> bankTransactionRepository)
        {
            _cashTransactionRepository = cashTransactionRepository;
            _bankTransactionRepository = bankTransactionRepository;
        }

        public async Task<Result<AccountBalanceResponse>> Handle(GetAccountBalanceQuery request, CancellationToken cancellationToken)
        {
            decimal balance = 0;
            var targetDate = request.EndDate ?? DateTime.Now;

            if (request.IsBank)
            {
                // Banka sorgusunu IQueryable olarak hazırla
                var query = _bankTransactionRepository.GetAll()
                    .Where(x => x.BankId == request.AccountId && x.Date <= targetDate && !x.IsDeleted);

                // Köprü metodunu kullan
                var txs = await _bankTransactionRepository.ToListAsync(query, cancellationToken);
                balance = txs.Sum(x => x.Debit - x.Credit);
            }
            else
            {
                // Kasa sorgusunu IQueryable olarak hazırla
                var query = _cashTransactionRepository.GetAll()
                    .Where(x => x.CashId == request.AccountId && x.Date <= targetDate && !x.IsDeleted);

                // Köprü metodunu kullan
                var txs = await _cashTransactionRepository.ToListAsync(query, cancellationToken);
                balance = txs.Sum(x => x.Debit - x.Credit);
            }

            return Result<AccountBalanceResponse>.Success(
                new AccountBalanceResponse(request.AccountId, balance, targetDate),
                "Bakiye hesaplandı."
            );
        }
    }
}
