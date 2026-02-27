using MediatR;
using MiniERP.Application.Interfaces;
using MiniERP.Domain.Common;
using MiniERP.Domain.Entities;

namespace MiniERP.Application.Features.Transactions.Commands.AddOpeningBalance
{
    public sealed class AddOpeningBalanceCommandHandler : IRequestHandler<AddOpeningBalanceCommand, Result<string>>
    {
        private readonly IRepository<CustomerTransaction> _customerTransactionRepository;
        private readonly IRepository<CashTransaction> _cashTransactionRepository;
        private readonly IRepository<BankTransaction> _bankTransactionRepository;
        private readonly IUnitOfWork _unitOfWork;

        public AddOpeningBalanceCommandHandler(
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

        public async Task<Result<string>> Handle(AddOpeningBalanceCommand request, CancellationToken cancellationToken)
        {
            if (request.Amount <= 0) return Result<string>.Failure("Açılış bakiyesi sıfırdan büyük olmalıdır.");

            var sharedTransactionId = Guid.NewGuid();
            decimal debit = request.IsDebit ? request.Amount : 0;
            decimal credit = request.IsDebit ? 0 : request.Amount;

            // 1. Kasaya Açılış Bakiyesi
            if (request.CashId is not null)
            {
                await _cashTransactionRepository.AddAsync(new CashTransaction
                {
                    TransactionId = sharedTransactionId,
                    CashId = request.CashId.Value,
                    Debit = debit,
                    Credit = credit,
                    Date = request.Date,
                    Description = request.Description ?? "Kasa Açılış Fişi"
                }, cancellationToken);
            }
            // 2. Bankaya Açılış Bakiyesi
            else if (request.BankId is not null)
            {
                await _bankTransactionRepository.AddAsync(new BankTransaction
                {
                    TransactionId = sharedTransactionId,
                    BankId = request.BankId.Value,
                    Debit = debit,
                    Credit = credit,
                    Date = request.Date,
                    Description = request.Description ?? "Banka Açılış Fişi"
                }, cancellationToken);
            }
            // 3. Cari Hesaba Açılış Bakiyesi
            else if (request.CustomerId is not null)
            {
                await _customerTransactionRepository.AddAsync(new CustomerTransaction
                {
                    TransactionId = sharedTransactionId,
                    CustomerId = request.CustomerId.Value,
                    Debit = debit,
                    Credit = credit,
                    Date = request.Date,
                    Description = request.Description ?? "Cari Açılış Fişi"
                }, cancellationToken);
            }
            else
            {
                return Result<string>.Failure("Lütfen bakiye eklenecek hesabı seçin.");
            }

            await _unitOfWork.SaveChangesAsync(cancellationToken);
            return Result<string>.Success(request.Description ?? "","Açılış bakiyesi başarıyla tanımlandı.");
        }
    }
}
