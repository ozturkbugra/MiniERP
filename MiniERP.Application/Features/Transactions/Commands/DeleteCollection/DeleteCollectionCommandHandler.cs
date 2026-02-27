using MediatR;
using MiniERP.Application.Interfaces;
using MiniERP.Domain.Common;
using MiniERP.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniERP.Application.Features.Transactions.Commands.DeleteCollection
{
    public sealed class DeleteCollectionCommandHandler : IRequestHandler<DeleteCollectionCommand, Result<string>>
    {
        private readonly IRepository<CustomerTransaction> _customerTransactionRepository;
        private readonly IRepository<CashTransaction> _cashTransactionRepository;
        private readonly IRepository<BankTransaction> _bankTransactionRepository;
        private readonly IUnitOfWork _unitOfWork;

        public DeleteCollectionCommandHandler(
            IRepository<CustomerTransaction> customerTransactionRepository,
            IRepository<CashTransaction> cashTransactionRepository,
            IRepository<BankTransaction> bankTransactionRepository,
            IUnitOfWork unitOfWork)
        {
            _customerTransactionRepository = customerTransactionRepository;
            _cashTransactionRepository = cashTransactionRepository;
            _bankTransactionRepository = bankTransactionRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<Result<string>> Handle(DeleteCollectionCommand request, CancellationToken cancellationToken)
        {
            // 1. TransactionId'ye sahip tüm parçaları farklı tablolardan topla
            var customerEntries = await _customerTransactionRepository.GetAllAsync(x => x.TransactionId == request.TransactionId, cancellationToken);
            var cashEntries = await _cashTransactionRepository.GetAllAsync(x => x.TransactionId == request.TransactionId, cancellationToken);
            var bankEntries = await _bankTransactionRepository.GetAllAsync(x => x.TransactionId == request.TransactionId, cancellationToken);

            // Hiç kayıt yoksa hata dön
            if (!customerEntries.Any() && !cashEntries.Any() && !bankEntries.Any())
                return Result<string>.Failure("İptal edilecek işlem bulunamadı.");

            // 2. Hepsini Soft Delete yap (Bakiyeye etkileri sıfırlanacak)
            foreach (var entry in customerEntries) entry.IsDeleted = true;
            foreach (var entry in cashEntries) entry.IsDeleted = true;
            foreach (var entry in bankEntries) entry.IsDeleted = true;

            // 3. UpdateRange ile toplu güncelleme (Repository yeteneğine göre)
            _customerTransactionRepository.UpdateRange(customerEntries);
            _cashTransactionRepository.UpdateRange(cashEntries);
            _bankTransactionRepository.UpdateRange(bankEntries);

            // 4. Tek seferde, Atomik Kayıt!
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Result<string>.Success(request.TransactionId.ToString() ,"Tahsilat işlemi ve bağlı tüm hareketler başarıyla iptal edildi.");
        }
    }
}
