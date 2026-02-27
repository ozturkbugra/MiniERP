using MediatR;
using MiniERP.Application.Interfaces;
using MiniERP.Domain.Common;
using MiniERP.Domain.Entities;

namespace MiniERP.Application.Features.Transactions.Queries.GetBankStatement
{
    public sealed class GetBankStatementQueryHandler : IRequestHandler<GetBankStatementQuery, Result<List<BankStatementResponse>>>
    {
        private readonly IRepository<BankTransaction> _bankTransactionRepository;

        public GetBankStatementQueryHandler(IRepository<BankTransaction> bankTransactionRepository)
        {
            _bankTransactionRepository = bankTransactionRepository;
        }

        public async Task<Result<List<BankStatementResponse>>> Handle(GetBankStatementQuery request, CancellationToken cancellationToken)
        {
            // IQueryable üzerinden sorguyu asenkron hazırlıyoruz
            var query = _bankTransactionRepository.GetAll()
                .Where(x => x.BankId == request.BankId && !x.IsDeleted) // Soft delete kontrolü
                .OrderBy(x => x.Date);

            // Repository'e eklediğimiz o meşhur ToListAsync köprüsü ile veritabanına gidiyoruz
            var transactions = await _bankTransactionRepository.ToListAsync(query, cancellationToken);

            var result = new List<BankStatementResponse>();
            decimal runningBalance = 0;

            // Finansal mantık: Her satırda bakiyeyi kümülatif olarak hesaplıyoruz
            foreach (var tx in transactions)
            {
                // Varlık hesabı olduğu için: Debit (Giriş) artırır, Credit (Çıkış) azaltır
                runningBalance += (tx.Debit - tx.Credit);

                result.Add(new BankStatementResponse(
                    tx.TransactionId,
                    tx.Date,
                    tx.Description ?? string.Empty,
                    tx.Debit,
                    tx.Credit,
                    runningBalance
                ));
            }

            return Result<List<BankStatementResponse>>.Success(result, "Banka Ekstresi Başarıyla Getirildi.");
        }
    }
}
