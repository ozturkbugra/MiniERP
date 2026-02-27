using MediatR;
using MiniERP.Application.Interfaces;
using MiniERP.Domain.Common;
using MiniERP.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniERP.Application.Features.Transactions.Commands.TransferMoney
{
    public sealed class TransferMoneyCommandHandler : IRequestHandler<TransferMoneyCommand, Result<string>>
    {
        private readonly IRepository<CashTransaction> _cashTransactionRepository;
        private readonly IRepository<BankTransaction> _bankTransactionRepository;
        private readonly IUnitOfWork _unitOfWork;

        public TransferMoneyCommandHandler(
            IRepository<CashTransaction> cashTransactionRepository,
            IRepository<BankTransaction> bankTransactionRepository,
            IUnitOfWork unitOfWork)
        {
            _cashTransactionRepository = cashTransactionRepository;
            _bankTransactionRepository = bankTransactionRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<Result<string>> Handle(TransferMoneyCommand request, CancellationToken cancellationToken)
        {
            // 1. Validasyonlar
            if (request.Amount <= 0) return Result<string>.Failure("Transfer tutarı sıfırdan büyük olmalıdır.");
            if (request.FromCashId == request.ToCashId && request.FromCashId != null)
                return Result<string>.Failure("Aynı kasalar arası transfer yapılamaz.");

            var sharedTransactionId = Guid.NewGuid();

            // 2. KAYNAK HESAPTAN ÇIKIŞ (Credit)
            if (request.FromCashId is not null)
            {
                await _cashTransactionRepository.AddAsync(new CashTransaction
                {
                    TransactionId = sharedTransactionId,
                    CashId = request.FromCashId.Value,
                    Debit = 0,
                    Credit = request.Amount,
                    Date = request.Date,
                    Description = request.Description
                }, cancellationToken);
            }
            else if (request.FromBankId is not null)
            {
                await _bankTransactionRepository.AddAsync(new BankTransaction
                {
                    TransactionId = sharedTransactionId,
                    BankId = request.FromBankId.Value,
                    Debit = 0,
                    Credit = request.Amount,
                    Date = request.Date,
                    Description = request.Description
                }, cancellationToken);
            }

            // 3. HEDEF HESABA GİRİŞ (Debit)
            if (request.ToCashId is not null)
            {
                await _cashTransactionRepository.AddAsync(new CashTransaction
                {
                    TransactionId = sharedTransactionId,
                    CashId = request.ToCashId.Value,
                    Debit = request.Amount,
                    Credit = 0,
                    Date = request.Date,
                    Description = request.Description
                }, cancellationToken);
            }
            else if (request.ToBankId is not null)
            {
                await _bankTransactionRepository.AddAsync(new BankTransaction
                {
                    TransactionId = sharedTransactionId,
                    BankId = request.ToBankId.Value,
                    Debit = request.Amount,
                    Credit = 0,
                    Date = request.Date,
                    Description = request.Description
                }, cancellationToken);
            }

            await _unitOfWork.SaveChangesAsync(cancellationToken);
            return Result<string>.Success(request.Description,"Transfer (Virman) işlemi başarıyla tamamlandı.");
        }
    }
}
