using MediatR;
using MiniERP.Application.Interfaces;
using MiniERP.Domain.Common;
using MiniERP.Domain.Entities;
using MiniERP.Domain.Enums;
using System;
using System.Data;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace MiniERP.Application.Features.Invoices.Commands.ApproveReturnInvoice
{
    public sealed class ApproveReturnInvoiceCommandHandler : IRequestHandler<ApproveReturnInvoiceCommand, Result<string>>
    {
        private readonly IRepository<Invoice> _invoiceRepository;
        private readonly IRepository<StockTransaction> _stockRepository;
        private readonly IRepository<CustomerTransaction> _customerRepository;
        private readonly IRepository<CashTransaction> _cashRepository;
        private readonly IRepository<BankTransaction> _bankRepository;
        private readonly IRepository<OrderDetail> _orderDetailRepository;
        private readonly IUnitOfWork _unitOfWork;

        public ApproveReturnInvoiceCommandHandler(
            IRepository<Invoice> invoiceRepository,
            IRepository<StockTransaction> stockRepository,
            IRepository<CustomerTransaction> customerRepository,
            IRepository<CashTransaction> cashRepository,
            IRepository<BankTransaction> bankRepository,
            IRepository<OrderDetail> orderDetailRepository,
            IUnitOfWork unitOfWork)
        {
            _invoiceRepository = invoiceRepository;
            _stockRepository = stockRepository;
            _customerRepository = customerRepository;
            _cashRepository = cashRepository;
            _bankRepository = bankRepository;
            _orderDetailRepository = orderDetailRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<Result<string>> Handle(ApproveReturnInvoiceCommand request, CancellationToken cancellationToken)
        {
            if (request.PaymentType == PaymentType.Credit && (request.CashId is not null || request.BankId is not null))
                return Result<string>.Failure("Açık hesapta (veresiye) kasa veya banka seçilemez.");

            await using var transaction = await _unitOfWork.BeginTransactionAsync(IsolationLevel.Serializable, cancellationToken);

            try
            {
                var invoice = await _invoiceRepository.GetByIdWithIncludesAsync(request.Id, cancellationToken, x => x.Details);

                if (invoice == null) return Result<string>.Failure("İade faturası bulunamadı.");
                if (invoice.Status != InvoiceStatus.Draft) return Result<string>.Failure("Sadece taslak faturada onay işlemi yapılabilir.");
                if (invoice.Type != InvoiceType.SalesReturn && invoice.Type != InvoiceType.PurchaseReturn)
                    return Result<string>.Failure("Bu işlem sadece iade faturaları için geçerlidir.");

                var isSalesReturn = invoice.Type == InvoiceType.SalesReturn;
                var sharedTransactionId = Guid.NewGuid();

                // 1. STOK HAREKETLERİ
                foreach (var detail in invoice.Details)
                {
                    // 🚀 Miktarı daima pozitif tutuyoruz, yönü aşağıda 'Type' belirleyecek
                    decimal absQuantity = Math.Abs(detail.Quantity);

                    // Alış İadesi yapıyorsak (Tedarikçiye mal geri yolluyorsak) depodan mal ÇIKAR. Yeterli stok var mı bakmalıyız.
                    if (!isSalesReturn)
                    {
                        var stockMoves = await _stockRepository.ToListAsync(_stockRepository.Where(x => x.ProductId == detail.ProductId && x.WarehouseId == detail.WarehouseId && !x.IsDeleted), cancellationToken);
                        var physicalBalance = stockMoves.Sum(x => x.Type == StockTransactionType.In ? x.Quantity : -x.Quantity);

                        var reservations = await _orderDetailRepository.ToListAsync(_orderDetailRepository.Where(x =>
                            x.ProductId == detail.ProductId &&
                            x.WarehouseId == detail.WarehouseId &&
                            x.Order!.Status == OrderStatus.Approved &&
                            !x.IsDeleted), cancellationToken);

                        var reservedAmount = reservations.Sum(x => x.Quantity);
                        var availableStock = physicalBalance - reservedAmount;

                        if (availableStock < absQuantity)
                            return Result<string>.Failure($"{detail.ProductId} için iade edilecek yeterli stok yok. Müsait: {availableStock}, İstenen: {absQuantity}");
                    }

                    // Satış İadesiyse Stok GİRER (In), Alış İadesiyse Stok ÇIKAR (Out)
                    await _stockRepository.AddAsync(new StockTransaction
                    {
                        TransactionId = sharedTransactionId,
                        DocumentNo = invoice.InvoiceNumber,
                        TransactionDate = invoice.InvoiceDate,
                        ProductId = detail.ProductId,
                        WarehouseId = detail.WarehouseId,
                        Quantity = absQuantity, // 🚀 Koruma kalkanı eklendi
                        UnitPrice = detail.UnitPrice,
                        Type = isSalesReturn ? StockTransactionType.In : StockTransactionType.Out,
                        Description = isSalesReturn ? "Satış İadesi - Stok Girişi" : "Alım İadesi - Stok Çıkışı",
                        CustomerId = invoice.CustomerId,
                        PaymentType = request.PaymentType
                    }, cancellationToken);
                }

                // 2. CARİ HAREKET (İade işlemi olduğu için normalin tam tersi)
                var mainTransaction = new CustomerTransaction
                {
                    TransactionId = sharedTransactionId,
                    Date = invoice.InvoiceDate,
                    Description = $"İade Faturası: {invoice.InvoiceNumber}",
                    CustomerId = invoice.CustomerId,
                    // Satış İadesi: Müşteri alacaklanır (Credit), Alış İadesi: Tedarikçi borçlanır (Debit)
                    Debit = isSalesReturn ? 0 : invoice.GrandTotal,
                    Credit = isSalesReturn ? invoice.GrandTotal : 0
                };
                await _customerRepository.AddAsync(mainTransaction, cancellationToken);

                // 3. PEŞİN ÖDEME İADESİ (Kasa/Banka Ters Kayıt)
                if (request.PaymentType != PaymentType.Credit)
                {
                    // Cariyi nötrleyen ters kayıt
                    var offsetTransaction = new CustomerTransaction
                    {
                        TransactionId = sharedTransactionId,
                        Date = invoice.InvoiceDate,
                        Description = $"İade Ödemesi: {invoice.InvoiceNumber}",
                        CustomerId = invoice.CustomerId,
                        Debit = isSalesReturn ? invoice.GrandTotal : 0,
                        Credit = isSalesReturn ? 0 : invoice.GrandTotal
                    };
                    await _customerRepository.AddAsync(offsetTransaction, cancellationToken);

                    // Kasa işlemi
                    if (request.PaymentType == PaymentType.Cash && request.CashId.HasValue)
                    {
                        await _cashRepository.AddAsync(new CashTransaction
                        {
                            TransactionId = sharedTransactionId,
                            CashId = request.CashId.Value,
                            Date = invoice.InvoiceDate,
                            // Satış İadesi: Biz müşteriye para iade ederiz, kasadan ÇIKAR (Credit)
                            // Alış İadesi: Tedarikçi bize para iade eder, kasaya GİRER (Debit)
                            Debit = isSalesReturn ? 0 : invoice.GrandTotal,
                            Credit = isSalesReturn ? invoice.GrandTotal : 0,
                            Description = $"İade İşlemi: {invoice.InvoiceNumber}"
                        }, cancellationToken);
                    }
                    // Banka işlemi
                    else if (request.PaymentType == PaymentType.Bank && request.BankId.HasValue)
                    {
                        await _bankRepository.AddAsync(new BankTransaction
                        {
                            TransactionId = sharedTransactionId,
                            BankId = request.BankId.Value,
                            Date = invoice.InvoiceDate,
                            Debit = isSalesReturn ? 0 : invoice.GrandTotal,
                            Credit = isSalesReturn ? invoice.GrandTotal : 0,
                            Description = $"İade İşlemi: {invoice.InvoiceNumber}"
                        }, cancellationToken);
                    }
                }

                // 4. İade Onaylandı, İşlemi Bitir
                invoice.Approve(sharedTransactionId, request.PaymentType);
                await _invoiceRepository.UpdateAsync(invoice, cancellationToken);

                await _unitOfWork.SaveChangesAsync(cancellationToken);
                await transaction.CommitAsync(cancellationToken);

                return Result<string>.Success(invoice.InvoiceNumber, "İade faturası onaylandı. Stok ve cari işlemler tersine çevrildi.");
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync(cancellationToken);
                return Result<string>.Failure($"Hata oluştu, iade işlemi iptal edildi: {ex.Message}");
            }
        }
    }
}