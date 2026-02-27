using MediatR;
using MiniERP.Application.Interfaces;
using MiniERP.Domain.Common;
using MiniERP.Domain.Entities;
using CustomerEntity = MiniERP.Domain.Entities.Customer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniERP.Application.Features.Transactions.Queries.GetCustomerListWithBalance
{
    public sealed class GetCustomerListWithBalanceQueryHandler : IRequestHandler<GetCustomerListWithBalanceQuery, Result<List<CustomerListWithBalanceResponse>>>
    {
        private readonly IRepository<CustomerEntity> _customerRepository;
        private readonly IRepository<CustomerTransaction> _transactionRepository;

        public GetCustomerListWithBalanceQueryHandler(IRepository<CustomerEntity> customerRepository, IRepository<CustomerTransaction> transactionRepository)
        {
            _customerRepository = customerRepository;
            _transactionRepository = transactionRepository;
        }

        public async Task<Result<List<CustomerListWithBalanceResponse>>> Handle(GetCustomerListWithBalanceQuery request, CancellationToken cancellationToken)
        {
            // 1. Tüm aktif müşterileri repository'deki temiz metotla alıyoruz
            var customers = await _customerRepository.GetAllAsync(cancellationToken);

            // 2. Hareketleri sorgu olarak hazırlıyoruz (Henüz DB'ye gitmedi)
            var transactionQuery = _transactionRepository.GetAll()
                .Where(x => !x.IsDeleted);

            var allTransactions = await _transactionRepository.ToListAsync(transactionQuery, cancellationToken);

            // 4. Hafızada (In-memory) gruplama ve bakiye hesabı
            var result = customers.Select(c => {
                var txs = allTransactions.Where(t => t.CustomerId == c.Id);
                var totalDebit = txs.Sum(t => t.Debit);
                var totalCredit = txs.Sum(t => t.Credit);

                return new CustomerListWithBalanceResponse(
                    c.Id,
                    c.Name,
                    totalDebit,
                    totalCredit,
                    totalDebit - totalCredit
                );
            }).OrderByDescending(x => Math.Abs(x.Balance)).ToList();

            return Result<List<CustomerListWithBalanceResponse>>.Success(result, "Müşteri Bakiyeleri Listelendi.");
        }
    }
}
