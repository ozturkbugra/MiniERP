using MediatR;
using MiniERP.Application.Interfaces;
using MiniERP.Domain.Common;
using MiniERP.Domain.Entities;

namespace MiniERP.Application.Features.Transactions.Queries.GetCashStatement
{
    public sealed class GetCashStatementQueryHandler : IRequestHandler<GetCashStatementQuery, Result<List<CashStatementResponse>>>
    {
        private readonly IRepository<CashTransaction> _cashTransactionRepository;

        public GetCashStatementQueryHandler(IRepository<CashTransaction> cashTransactionRepository)
        {
            _cashTransactionRepository = cashTransactionRepository;
        }

        public async Task<Result<List<CashStatementResponse>>> Handle(GetCashStatementQuery request, CancellationToken cancellationToken)
        {
            var query = _cashTransactionRepository.GetAll()
                .Where(x => x.CashId == request.CashId && !x.IsDeleted) // Soft delete kontrolü
                .OrderBy(x => x.Date);

            var transactions = await _cashTransactionRepository.ToListAsync(query, cancellationToken);

            var result = new List<CashStatementResponse>();
            decimal runningBalance = 0;

            foreach (var tx in transactions)
            {
                // Kasaya giriş (Debit) artırır, çıkış (Credit) azaltır
                runningBalance += (tx.Debit - tx.Credit);

                result.Add(new CashStatementResponse(
                    tx.TransactionId,
                    tx.Date,
                    tx.Description ?? "Kasa Hareketi",
                    tx.Debit,
                    tx.Credit,
                    runningBalance
                ));
            }

            return Result<List<CashStatementResponse>>.Success(result, "Kasa Ekstresi Başarıyla Hazırlandı.");
        }
    }
}
