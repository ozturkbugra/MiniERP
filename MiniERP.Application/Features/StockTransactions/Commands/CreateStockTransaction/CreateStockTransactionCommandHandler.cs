using MediatR;
using MiniERP.Application.Interfaces;
using MiniERP.Domain.Common;
using MiniERP.Domain.Entities;
using MiniERP.Domain.Enums;

namespace MiniERP.Application.Features.StockTransactions.Commands.CreateStockTransaction
{
    public sealed class CreateStockTransactionCommandHandler
        : IRequestHandler<CreateStockTransactionCommand, Result<string>>
    {
        private readonly IRepository<StockTransaction> _stockRepository;
        private readonly IRepository<CustomerTransaction> _customerRepository;
        private readonly IRepository<CashTransaction> _cashRepository;
        private readonly IRepository<BankTransaction> _bankRepository;
        private readonly IUnitOfWork _unitOfWork;

        public CreateStockTransactionCommandHandler(
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

        public async Task<Result<string>> Handle(CreateStockTransactionCommand request, CancellationToken cancellationToken)
        {
            // 1. Validasyonlar (Kapıdaki güvenlik)
            if (request.PaymentType == PaymentType.Credit && (request.CashId is not null || request.BankId is not null))
                return Result<string>.Failure("Veresiye işlemde kasa veya banka seçilemez.");


            if (request.Quantity <= 0 || request.UnitPrice <= 0)
                return Result<string>.Failure("Miktar ve birim fiyat sıfırdan büyük olmalıdır.");

            if (request.PaymentType == PaymentType.Cash && request.CashId is null)
                return Result<string>.Failure("Nakit işlem için kasa seçilmelidir.");

            if (request.PaymentType == PaymentType.Bank && request.BankId is null)
                return Result<string>.Failure("Banka işlem için banka seçilmelidir.");

            var sharedTransactionId = Guid.NewGuid();
            var totalAmount = request.Quantity * request.UnitPrice;
            var isStockIn = request.Type == StockTransactionType.In;

            // 2. STOK HAREKETİ (Fiziksel Ayak)
            var stockTransaction = new StockTransaction
            {
                DocumentNo = request.DocumentNo,
                TransactionDate = request.TransactionDate,
                Quantity = request.Quantity,
                UnitPrice = request.UnitPrice,
                Description = request.Description,
                Type = request.Type,
                TransactionId = sharedTransactionId,
                PaymentType = request.PaymentType,
                ProductId = request.ProductId,
                WarehouseId = request.WarehouseId,
                CustomerId = request.CustomerId
            };
            await _stockRepository.AddAsync(stockTransaction, cancellationToken);

            // 3. CARİ HAREKETİ - FATURA KAYDI (Alacak/Borç Oluşturma)
            // Alış (In) -> Biz borçlanırız, Cari alacaklanır (Credit)
            // Satış (Out) -> Biz alacaklanırız, Cari borçlanır (Debit)
            var invoiceEntry = new CustomerTransaction
            {
                TransactionId = sharedTransactionId,
                Date = request.TransactionDate,
                Description = request.Description ?? (isStockIn ? "Mal Alım Faturası" : "Mal Satış Faturası"),
                CustomerId = request.CustomerId,
                Debit = isStockIn ? 0 : totalAmount,
                Credit = isStockIn ? totalAmount : 0
            };
            invoiceEntry.Validate();
            await _customerRepository.AddAsync(invoiceEntry, cancellationToken);

            // 4. EĞER PEŞİN ÖDEME VARSA (Kasa veya Banka)
            if (request.PaymentType != PaymentType.Credit)
            {
                // A) CARİ HAREKETİ - ÖDEME KAYDI (Cariyi kapatan ters kayıt)
                // Alış yaptıysak (In) cariye ödeme yaparız (Cari Debit)
                // Satış yaptıysak (Out) tahsilat yaparız (Cari Credit)
                var paymentOffset = new CustomerTransaction
                {
                    TransactionId = sharedTransactionId,
                    Date = request.TransactionDate,
                    Description = request.Description ?? (isStockIn ? "Peşin Ödeme (Stok)" : "Peşin Tahsilat (Stok)"),
                    CustomerId = request.CustomerId,
                    Debit = isStockIn ? totalAmount : 0, // Alışta borcumuz azalır
                    Credit = isStockIn ? 0 : totalAmount  // Satışta alacağımız azalır
                };
                paymentOffset.Validate();
                await _customerRepository.AddAsync(paymentOffset, cancellationToken);

                // B) KASA VEYA BANKA KAYDI (Parasal Çıkış/Giriş)
                if (request.PaymentType == PaymentType.Cash)
                {
                    var cashEntry = new CashTransaction
                    {
                        TransactionId = sharedTransactionId,
                        Date = request.TransactionDate,
                        Description = request.Description ?? (isStockIn ? "Stok Ödemesi" : "Stok Tahsilatı"),
                        CashId = request.CashId!.Value,
                        Debit = isStockIn ? 0 : totalAmount, // Satışta kasa artar
                        Credit = isStockIn ? totalAmount : 0 // Alışta kasa azalır
                    };
                    cashEntry.Validate();
                    await _cashRepository.AddAsync(cashEntry, cancellationToken);
                }
                else if (request.PaymentType == PaymentType.Bank)
                {
                    var bankEntry = new BankTransaction
                    {
                        TransactionId = sharedTransactionId,
                        Date = request.TransactionDate,
                        Description = request.Description ?? (isStockIn ? "Stok Havalesi" : "Stok Tahsilatı (Banka)"),
                        BankId = request.BankId!.Value,
                        Debit = isStockIn ? 0 : totalAmount,
                        Credit = isStockIn ? totalAmount : 0
                    };
                    bankEntry.Validate();
                    await _bankRepository.AddAsync(bankEntry, cancellationToken);
                }
            }

            
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            string transactionTypeName = isStockIn ? "giriş" : "çıkış";
            return Result<string>.Success(request.DocumentNo, $"Stok {transactionTypeName} işlemi başarıyla tamamlandı.");
        }
    }
}