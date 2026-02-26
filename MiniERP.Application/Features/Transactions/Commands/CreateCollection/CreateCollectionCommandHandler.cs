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

namespace MiniERP.Application.Features.Transactions.Commands.CreateCollection
{
    public sealed class CreateCollectionCommandHandler : IRequestHandler<CreateCollectionCommand, Result<string>>
    {
        private readonly IRepository<CustomerTransaction> _customerTransactionRepository;
        private readonly IRepository<CashTransaction> _cashTransactionRepository;
        private readonly IRepository<BankTransaction> _bankTransactionRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public CreateCollectionCommandHandler(
            IRepository<CustomerTransaction> customerTransactionRepository,
            IRepository<CashTransaction> cashTransactionRepository,
            IRepository<BankTransaction> bankTransactionRepository,
            IUnitOfWork unitOfWork,
            IMapper mapper)
        {
            _customerTransactionRepository = customerTransactionRepository;
            _cashTransactionRepository = cashTransactionRepository;
            _bankTransactionRepository = bankTransactionRepository;
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<Result<string>> Handle(CreateCollectionCommand request, CancellationToken cancellationToken)
        {
            // 1. İş Kuralı Kontrolleri
            if (request.Amount <= 0) return Result<string>.Failure("Tahsilat tutarı sıfırdan büyük olmalıdır.");
            if (request.CashId is null && request.BankId is null) return Result<string>.Failure("Kasa veya Banka seçilmelidir.");

            // 2. Müşteri Hareketini Map'le ve Ekle
            var customerTransaction = _mapper.Map<CustomerTransaction>(request);
            await _customerTransactionRepository.AddAsync(customerTransaction, cancellationToken);

            // 3. Kasa veya Banka Hareketini Map'le ve Ekle
            if (request.CashId is not null)
            {
                var cashTransaction = _mapper.Map<CashTransaction>(request);
                await _cashTransactionRepository.AddAsync(cashTransaction, cancellationToken);
            }
            else if (request.BankId is not null)
            {
                var bankTransaction = _mapper.Map<BankTransaction>(request);
                await _bankTransactionRepository.AddAsync(bankTransaction, cancellationToken);
            }

            // 4. Atomik Kayıt
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Result<string>.Success(request.Description,"Tahsilat işlemi başarıyla kaydedildi.");
        }
    }
}
