using MediatR;
using MiniERP.Application.Interfaces;
using MiniERP.Domain.Common;
using MiniERP.Domain.Entities;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniERP.Application.Features.Transactions.Queries.GetCustomerStatement
{
    public sealed class GetCustomerStatementQueryHandler : IRequestHandler<GetCustomerStatementQuery, Result<List<CustomerStatementDto>>>
    {
        private readonly IRepository<CustomerTransaction> _customerTransactionRepository;

        public GetCustomerStatementQueryHandler(IRepository<CustomerTransaction> customerTransactionRepository)
        {
            _customerTransactionRepository = customerTransactionRepository;
        }

        public async Task<Result<List<CustomerStatementDto>>> Handle(GetCustomerStatementQuery request, CancellationToken cancellationToken)
        {
            // 1. Müşterinin tüm geçerli (silinmemiş) hareketlerini tarihe göre getir
            var query = _customerTransactionRepository.GetAll()
            .Where(x => x.CustomerId == request.CustomerId && !x.IsDeleted)
            .OrderBy(x => x.Date);
            var transactions = await _customerTransactionRepository.ToListAsync(query, cancellationToken);

            var result = new List<CustomerStatementDto>();
            decimal runningBalance = 0;

            // 2. Yürüyen bakiye hesabı
            foreach (var tx in transactions)
            {
                runningBalance += (tx.Debit - tx.Credit); // Borç bakiyeyi artırır, alacak azaltır

                result.Add(new CustomerStatementDto
                (
                    tx.TransactionId,
                    tx.Date,
                    tx.Description,
                    tx.Debit,
                    tx.Credit,
                    runningBalance
                ));
            }

            return Result<List<CustomerStatementDto>>.Success(result, "Cari Ekstresi Başarılı Bir Şekilde Getirildi.");
        }
    }
}
