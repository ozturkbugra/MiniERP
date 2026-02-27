using AutoMapper;
using MediatR;
using MiniERP.Application.Interfaces;
using MiniERP.Domain.Common;
using MiniERP.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniERP.Application.Features.Transactions.Commands.MakePayment
{
    public sealed class MakePaymentCommandHandler : IRequestHandler<MakePaymentCommand, Result<string>>
    {
        private readonly IRepository<CustomerTransaction> _customerTransactionRepository;
        private readonly IRepository<CashTransaction> _cashTransactionRepository;
        private readonly IRepository<BankTransaction> _bankTransactionRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public MakePaymentCommandHandler(IRepository<CustomerTransaction> customerTransactionRepository, IRepository<CashTransaction> cashTransactionRepository, IRepository<BankTransaction> bankTransactionRepository, IUnitOfWork unitOfWork, IMapper mapper)
        {
            _customerTransactionRepository = customerTransactionRepository;
            _cashTransactionRepository = cashTransactionRepository;
            _bankTransactionRepository = bankTransactionRepository;
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<Result<string>> Handle(MakePaymentCommand request, CancellationToken cancellationToken)
        {
            if (request.Amount <= 0) return Result<string>.Failure("Ödeme tutarı sıfırdan büyük olmalıdır.");

            var sharedTransactionId = Guid.NewGuid();

            // 1. Cari Hareketi (Debit - Borcumuz azalıyor)
            var customerTransaction = _mapper.Map<CustomerTransaction>(request);
            customerTransaction.TransactionId = sharedTransactionId;
            await _customerTransactionRepository.AddAsync(customerTransaction, cancellationToken);

            // 2. Kasa veya Banka Hareketi (Credit - Para çıkıyor)
            if (request.CashId is not null)
            {
                var cashTransaction = _mapper.Map<CashTransaction>(request);
                cashTransaction.TransactionId = sharedTransactionId;
                await _cashTransactionRepository.AddAsync(cashTransaction, cancellationToken);
            }
            else if (request.BankId is not null)
            {
                var bankTransaction = _mapper.Map<BankTransaction>(request);
                bankTransaction.TransactionId = sharedTransactionId;
                await _bankTransactionRepository.AddAsync(bankTransaction, cancellationToken);
            }

            await _unitOfWork.SaveChangesAsync(cancellationToken);
            return Result<string>.Success(request.Description,"Ödeme işlemi başarıyla kaydedildi.");
        }
    }
}
