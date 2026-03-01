using MediatR;
using MiniERP.Application.Interfaces;
using MiniERP.Domain.Common;
using MiniERP.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniERP.Application.Features.StockTransactions.Commands.DeleteStockTransaction
{
    public sealed class DeleteStockTransactionCommandHandler
    : IRequestHandler<DeleteStockTransactionCommand, Result<string>>
    {
        private readonly IRepository<StockTransaction> _stockRepository;
        private readonly IRepository<CustomerTransaction> _customerRepository;
        private readonly IRepository<CashTransaction> _cashRepository;
        private readonly IRepository<BankTransaction> _bankRepository;
        private readonly IUnitOfWork _unitOfWork;

        public DeleteStockTransactionCommandHandler(
            IRepository<StockTransaction> stockRepository,
            IRepository<CustomerTransaction> customerRepository,
            IRepository<CashTransaction> cashRepository,
            IRepository<BankTransaction> bankRepository,
            IUnitOfWork unitOfWork)
        {
            _stockRepository = stockRepository;
            _customerRepository = customerRepository;
            _cashRepository = cashRepository;
            _bankRepository = bankRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<Result<string>> Handle(DeleteStockTransactionCommand request, CancellationToken cancellationToken)
        {
            var stockEntry = await _stockRepository.GetByIdAsync(request.Id, cancellationToken);
            if (stockEntry == null)
                return Result<string>.Failure("Silinecek stok hareketi bulunamadı.");

            var tid = stockEntry.TransactionId;


            stockEntry.IsDeleted = true;

            var customerEntries = await _customerRepository.GetAllAsync(x => x.TransactionId == tid, cancellationToken);
            foreach (var entry in customerEntries) entry.IsDeleted = true;

            var cashEntries = await _cashRepository.GetAllAsync(x => x.TransactionId == tid, cancellationToken);
            foreach (var entry in cashEntries) entry.IsDeleted = true;

            var bankEntries = await _bankRepository.GetAllAsync(x => x.TransactionId == tid, cancellationToken);
            foreach (var entry in bankEntries) entry.IsDeleted = true;

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Result<string>.Success(stockEntry.DocumentNo, "Stok hareketi ve ilişkili tüm finansal kayıtlar başarıyla iptal edildi.");
        }
    }
}
